using AutoMapper;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Application.DTOs.RecipeImage.Request;
using Foodiya.Application.DTOs.RecipeImage.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Application.Interfaces.Services;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using Foodiya.Domain.Specifications.RecipeImages;

namespace Foodiya.Application.Services;

public sealed class RecipeImageService : IRecipeImageService
{
    private readonly IResourceAuthorization _authorization;
    private readonly IRecipeImageRepository _recipeImageRepo;
    private readonly IGenericRepository<Recipe> _recipeRepo;
    private readonly IMapper _mapper;
    private readonly IRecipeImageFactory _factory;

    public RecipeImageService(
        IResourceAuthorization authorization,
        IRecipeImageRepository recipeImageRepo,
        IGenericRepository<Recipe> recipeRepo,
        IMapper mapper,
        IRecipeImageFactory factory)
    {
        _authorization = authorization;
        _recipeImageRepo = recipeImageRepo;
        _recipeRepo = recipeRepo;
        _mapper = mapper;
        _factory = factory;
    }

    public async Task<RecipeImageDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default)
    {
        var recipeImage = await _recipeImageRepo.GetByUuidAsync(uuid, ct);
        if (recipeImage is null) return null;
        await _authorization.RequireRecipeReadAsync(recipeImage.RecipeId, ct);
        return await GetByIdInternalAsync(recipeImage.Id, ct);
    }

    public async Task<PaginatedResponse<RecipeImageDetailResponse>> ListAsync(
        int page,
        int pageSize,
        Guid? recipeUuid,
        bool? isPrimary,
        string? search,
        CancellationToken ct = default)
    {
        var recipeId = recipeUuid.HasValue
            ? (await _recipeRepo.GetRequiredByUuidAsync(recipeUuid.Value, nameof(Recipe), ct)).Id
            : (int?)null;

        var listSpec = new RecipeImageListSpecification(page, pageSize, recipeId, isPrimary, search);
        var countSpec = new RecipeImageCountSpecification(recipeId, isPrimary, search);

        var access = await _authorization.GetReadAccessAsync(ct);
        listSpec.RestrictTo(access.Children((RecipeImage child) => child.Recipe, allowUnlisted: recipeUuid.HasValue));
        countSpec.RestrictTo(access.Children((RecipeImage child) => child.Recipe, allowUnlisted: recipeUuid.HasValue));

        var items = await _recipeImageRepo.ListAsync(listSpec, ct);
        var totalCount = await _recipeImageRepo.CountAsync(countSpec, ct);

        return new PaginatedResponse<RecipeImageDetailResponse>
        {
            Data = _mapper.Map<IReadOnlyList<RecipeImageDetailResponse>>(items),
            Meta = new PaginationMeta
            {
                Page = page,
                Take = pageSize,
                ItemCount = totalCount
            }
        };
    }

    public async Task<RecipeImageDetailResponse> CreateAsync(CreateRecipeImageRequest request, CancellationToken ct = default)
    {
        request.RecipeId = (await _recipeRepo.GetRequiredByUuidAsync(request.RecipeUuid, nameof(Recipe), ct)).Id;
        await _authorization.RequireRecipeOwnerAsync(request.RecipeId, ct);
        await EnsureRecipeExistsAsync(request.RecipeId, ct);

        if (request.IsPrimary)
            await ClearPrimaryImageAsync(request.RecipeId, null, ct);

        var recipeImage = _factory.Create(request);

        await _recipeImageRepo.InsertAsync(recipeImage, ct);
        await _recipeImageRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(recipeImage.Id, ct))!;
    }

    public async Task<RecipeImageDetailResponse> UpdateAsync(Guid uuid, UpdateRecipeImageRequest request, CancellationToken ct = default)
    {
        var recipeImage = await _recipeImageRepo.GetRequiredByUuidAsync(uuid, nameof(RecipeImage), ct);

        await _authorization.RequireRecipeOwnerAsync(recipeImage.RecipeId, ct);
        await EnsureRecipeExistsAsync(recipeImage.RecipeId, ct);

        _factory.Update(recipeImage, request);

        if (recipeImage.IsPrimary)
            await ClearPrimaryImageAsync(recipeImage.RecipeId, recipeImage.Id, ct);

        _recipeImageRepo.Update(recipeImage);
        await _recipeImageRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(recipeImage.Id, ct))!;
    }

    public async Task DeleteAsync(Guid uuid, CancellationToken ct = default)
    {
        var recipeImage = await _recipeImageRepo.GetRequiredByUuidAsync(uuid, nameof(RecipeImage), ct);

        await _authorization.RequireRecipeOwnerAsync(recipeImage.RecipeId, ct);
        _recipeImageRepo.Delete(recipeImage);
        await _recipeImageRepo.SaveAsync(ct);
    }

    private async Task<RecipeImageDetailResponse?> GetByIdInternalAsync(int id, CancellationToken ct)
    {
        var recipeImage = await _recipeImageRepo.GetSingleAsync(new RecipeImageByIdSpecification(id), ct);
        return recipeImage is null ? null : _mapper.Map<RecipeImageDetailResponse>(recipeImage);
    }

    private async Task EnsureRecipeExistsAsync(int recipeId, CancellationToken ct)
    {
        var recipe = await _recipeRepo.GetByIdAsync(recipeId, ct: ct)
            ?? throw new FoodiyaNotFoundException($"Recipe with ID {recipeId} not found.");

        if (recipe.DeletedAt is not null)
            throw new FoodiyaBadRequestException($"Recipe with ID {recipeId} is deleted and cannot have images.");
    }

    private Task ClearPrimaryImageAsync(int recipeId, int? excludeImageId, CancellationToken ct)
    {
        var existingPrimaryImages = _recipeImageRepo.GetAll()
            .Where(image => image.RecipeId == recipeId && image.IsPrimary && (!excludeImageId.HasValue || image.Id != excludeImageId.Value))
            .ToList();

        foreach (var image in existingPrimaryImages)
            image.IsPrimary = false;

        return Task.CompletedTask;
    }
}
