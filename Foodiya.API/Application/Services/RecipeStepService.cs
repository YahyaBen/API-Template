using AutoMapper;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Application.DTOs.RecipeStep.Request;
using Foodiya.Application.DTOs.RecipeStep.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Application.Interfaces.Services;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using Foodiya.Domain.Specifications.RecipeSteps;

namespace Foodiya.Application.Services;

public sealed class RecipeStepService : IRecipeStepService
{
    private readonly IResourceAuthorization _authorization;
    private readonly IRecipeStepRepository _recipeStepRepo;
    private readonly IGenericRepository<Recipe> _recipeRepo;
    private readonly IMapper _mapper;
    private readonly IRecipeStepFactory _factory;

    public RecipeStepService(
        IResourceAuthorization authorization,
        IRecipeStepRepository recipeStepRepo,
        IGenericRepository<Recipe> recipeRepo,
        IMapper mapper,
        IRecipeStepFactory factory)
    {
        _authorization = authorization;
        _recipeStepRepo = recipeStepRepo;
        _recipeRepo = recipeRepo;
        _mapper = mapper;
        _factory = factory;
    }

    public async Task<RecipeStepDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default)
    {
        var recipeStep = await _recipeStepRepo.GetByUuidAsync(uuid, ct);
        if (recipeStep is null) return null;
        await _authorization.RequireRecipeReadAsync(recipeStep.RecipeId, ct);
        return await GetByIdInternalAsync(recipeStep.Id, ct);
    }

    public async Task<PaginatedResponse<RecipeStepDetailResponse>> ListAsync(
        int page,
        int pageSize,
        Guid? recipeUuid,
        string? search,
        CancellationToken ct = default)
    {
        var recipeId = recipeUuid.HasValue
            ? (await _recipeRepo.GetRequiredByUuidAsync(recipeUuid.Value, nameof(Recipe), ct)).Id
            : (int?)null;

        var listSpec = new RecipeStepListSpecification(page, pageSize, recipeId, search);
        var countSpec = new RecipeStepCountSpecification(recipeId, search);

        var access = await _authorization.GetReadAccessAsync(ct);
        listSpec.RestrictTo(access.Children((RecipeStep child) => child.Recipe, allowUnlisted: recipeUuid.HasValue));
        countSpec.RestrictTo(access.Children((RecipeStep child) => child.Recipe, allowUnlisted: recipeUuid.HasValue));

        var items = await _recipeStepRepo.ListAsync(listSpec, ct);
        var totalCount = await _recipeStepRepo.CountAsync(countSpec, ct);

        return new PaginatedResponse<RecipeStepDetailResponse>
        {
            Data = _mapper.Map<IReadOnlyList<RecipeStepDetailResponse>>(items),
            Meta = new PaginationMeta
            {
                Page = page,
                Take = pageSize,
                ItemCount = totalCount
            }
        };
    }

    public async Task<RecipeStepDetailResponse> CreateAsync(CreateRecipeStepItemRequest request, CancellationToken ct = default)
    {
        request.RecipeId = (await _recipeRepo.GetRequiredByUuidAsync(request.RecipeUuid, nameof(Recipe), ct)).Id;
        await _authorization.RequireRecipeOwnerAsync(request.RecipeId, ct);
        await EnsureRecipeExistsAsync(request.RecipeId, ct);

        var recipeStep = _factory.Create(request);

        await _recipeStepRepo.InsertAsync(recipeStep, ct);
        await _recipeStepRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(recipeStep.Id, ct))!;
    }

    public async Task<RecipeStepDetailResponse> UpdateAsync(Guid uuid, UpdateRecipeStepItemRequest request, CancellationToken ct = default)
    {
        var recipeStep = await _recipeStepRepo.GetRequiredByUuidAsync(uuid, nameof(RecipeStep), ct);

        await _authorization.RequireRecipeOwnerAsync(recipeStep.RecipeId, ct);
        await EnsureRecipeExistsAsync(recipeStep.RecipeId, ct);

        _factory.Update(recipeStep, request);

        _recipeStepRepo.Update(recipeStep);
        await _recipeStepRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(recipeStep.Id, ct))!;
    }

    public async Task DeleteAsync(Guid uuid, CancellationToken ct = default)
    {
        var recipeStep = await _recipeStepRepo.GetRequiredByUuidAsync(uuid, nameof(RecipeStep), ct);

        await _authorization.RequireRecipeOwnerAsync(recipeStep.RecipeId, ct);
        _recipeStepRepo.Delete(recipeStep);
        await _recipeStepRepo.SaveAsync(ct);
    }

    private async Task<RecipeStepDetailResponse?> GetByIdInternalAsync(int id, CancellationToken ct)
    {
        var recipeStep = await _recipeStepRepo.GetSingleAsync(new RecipeStepByIdSpecification(id), ct);
        return recipeStep is null ? null : _mapper.Map<RecipeStepDetailResponse>(recipeStep);
    }

    private async Task EnsureRecipeExistsAsync(int recipeId, CancellationToken ct)
    {
        var recipe = await _recipeRepo.GetByIdAsync(recipeId, ct: ct)
            ?? throw new FoodiyaNotFoundException($"Recipe with ID {recipeId} not found.");

        if (recipe.DeletedAt is not null)
            throw new FoodiyaBadRequestException($"Recipe with ID {recipeId} is deleted and cannot have steps.");
    }
}
