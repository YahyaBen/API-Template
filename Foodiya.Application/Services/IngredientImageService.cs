using AutoMapper;
using Foodiya.Application.DTOs.IngredientImage.Request;
using Foodiya.Application.DTOs.IngredientImage.Response;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Application.Interfaces.Services;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using Foodiya.Domain.Specifications.IngredientImages;

namespace Foodiya.Application.Services;

public sealed class IngredientImageService : IIngredientImageService
{
    private readonly IIngredientImageRepository _ingredientImageRepo;
    private readonly IGenericRepository<Ingredient> _ingredientRepo;
    private readonly IMapper _mapper;
    private readonly IIngredientImageFactory _factory;

    public IngredientImageService(
        IIngredientImageRepository ingredientImageRepo,
        IGenericRepository<Ingredient> ingredientRepo,
        IMapper mapper,
        IIngredientImageFactory factory)
    {
        _ingredientImageRepo = ingredientImageRepo;
        _ingredientRepo = ingredientRepo;
        _mapper = mapper;
        _factory = factory;
    }

    public async Task<IngredientImageDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default)
    {
        var ingredientImage = await _ingredientImageRepo.GetByUuidAsync(uuid, ct);
        return ingredientImage is null ? null : await GetByIdInternalAsync(ingredientImage.Id, ct);
    }

    public async Task<PaginatedResponse<IngredientImageDetailResponse>> ListAsync(
        int page,
        int pageSize,
        Guid? ingredientUuid,
        bool? isPrimary,
        string? search,
        CancellationToken ct = default)
    {
        var ingredientId = ingredientUuid.HasValue
            ? (await _ingredientRepo.GetRequiredByUuidAsync(ingredientUuid.Value, nameof(Ingredient), ct)).Id
            : (int?)null;

        var listSpec = new IngredientImageListSpecification(page, pageSize, ingredientId, isPrimary, search);
        var countSpec = new IngredientImageCountSpecification(ingredientId, isPrimary, search);

        var items = await _ingredientImageRepo.ListAsync(listSpec, ct);
        var totalCount = await _ingredientImageRepo.CountAsync(countSpec, ct);

        return new PaginatedResponse<IngredientImageDetailResponse>
        {
            Data = _mapper.Map<IReadOnlyList<IngredientImageDetailResponse>>(items),
            Meta = new PaginationMeta
            {
                Page = page,
                Take = pageSize,
                ItemCount = totalCount
            }
        };
    }

    public async Task<IngredientImageDetailResponse> CreateAsync(CreateIngredientImageRequest request, CancellationToken ct = default)
    {
        request.IngredientId = (await _ingredientRepo.GetRequiredByUuidAsync(request.IngredientUuid, nameof(Ingredient), ct)).Id;
        await EnsureIngredientExistsAsync(request.IngredientId, ct);

        if (request.IsPrimary)
            await ClearPrimaryImageAsync(request.IngredientId, null, ct);

        var ingredientImage = _factory.Create(request);

        await _ingredientImageRepo.InsertAsync(ingredientImage, ct);
        await _ingredientImageRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(ingredientImage.Id, ct))!;
    }

    public async Task<IngredientImageDetailResponse> UpdateAsync(Guid uuid, UpdateIngredientImageRequest request, CancellationToken ct = default)
    {
        var ingredientImage = await _ingredientImageRepo.GetRequiredByUuidAsync(uuid, nameof(IngredientImage), ct);

        await EnsureIngredientExistsAsync(ingredientImage.IngredientId, ct);

        _factory.Update(ingredientImage, request);

        if (ingredientImage.IsPrimary)
            await ClearPrimaryImageAsync(ingredientImage.IngredientId, ingredientImage.Id, ct);

        _ingredientImageRepo.Update(ingredientImage);
        await _ingredientImageRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(ingredientImage.Id, ct))!;
    }

    public async Task DeleteAsync(Guid uuid, CancellationToken ct = default)
    {
        var ingredientImage = await _ingredientImageRepo.GetRequiredByUuidAsync(uuid, nameof(IngredientImage), ct);

        _ingredientImageRepo.Delete(ingredientImage);
        await _ingredientImageRepo.SaveAsync(ct);
    }

    private async Task<IngredientImageDetailResponse?> GetByIdInternalAsync(int id, CancellationToken ct)
    {
        var ingredientImage = await _ingredientImageRepo.GetSingleAsync(new IngredientImageByIdSpecification(id), ct);
        return ingredientImage is null ? null : _mapper.Map<IngredientImageDetailResponse>(ingredientImage);
    }

    private async Task EnsureIngredientExistsAsync(int ingredientId, CancellationToken ct)
    {
        _ = await _ingredientRepo.GetByIdAsync(ingredientId, ct: ct)
            ?? throw new FoodiyaNotFoundException($"Ingredient with ID {ingredientId} not found.");
    }

    private Task ClearPrimaryImageAsync(int ingredientId, int? excludeImageId, CancellationToken ct)
    {
        var existingPrimaryImages = _ingredientImageRepo.GetAll()
            .Where(image => image.IngredientId == ingredientId && image.IsPrimary && (!excludeImageId.HasValue || image.Id != excludeImageId.Value))
            .ToList();

        foreach (var image in existingPrimaryImages)
            image.IsPrimary = false;

        return Task.CompletedTask;
    }
}
