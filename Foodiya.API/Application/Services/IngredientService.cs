using AutoMapper;
using Foodiya.Application.DTOs.Ingredient.Request;
using Foodiya.Application.DTOs.Ingredient.Response;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Application.Interfaces.Services;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using Foodiya.Domain.Specifications.Ingredients;

namespace Foodiya.Application.Services;

public sealed class IngredientService : IIngredientService
{
    private readonly IIngredientRepository _ingredientRepo;
    private readonly IGenericRepository<IngredientType> _ingredientTypeRepo;
    private readonly IGenericRepository<Unit> _unitRepo;
    private readonly IGenericRepository<RecipeIngredient> _recipeIngredientRepo;
    private readonly IGenericRepository<IngredientImage> _ingredientImageRepo;
    private readonly IMapper _mapper;
    private readonly IIngredientFactory _factory;

    public IngredientService(
        IIngredientRepository ingredientRepo,
        IGenericRepository<IngredientType> ingredientTypeRepo,
        IGenericRepository<Unit> unitRepo,
        IGenericRepository<RecipeIngredient> recipeIngredientRepo,
        IGenericRepository<IngredientImage> ingredientImageRepo,
        IMapper mapper,
        IIngredientFactory factory)
    {
        _ingredientRepo = ingredientRepo;
        _ingredientTypeRepo = ingredientTypeRepo;
        _unitRepo = unitRepo;
        _recipeIngredientRepo = recipeIngredientRepo;
        _ingredientImageRepo = ingredientImageRepo;
        _mapper = mapper;
        _factory = factory;
    }

    public async Task<IngredientDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default)
    {
        var ingredient = await _ingredientRepo.GetByUuidAsync(uuid, ct);
        return ingredient is null ? null : await GetByIdInternalAsync(ingredient.Id, ct);
    }

    public async Task<PaginatedResponse<IngredientDetailResponse>> ListAsync(
        int page,
        int pageSize,
        Guid? ingredientTypeUuid,
        bool? isActive,
        string? search,
        CancellationToken ct = default)
    {
        var ingredientTypeId = ingredientTypeUuid.HasValue
            ? (await _ingredientTypeRepo.GetRequiredByUuidAsync(ingredientTypeUuid.Value, nameof(IngredientType), ct)).Id
            : (int?)null;

        var listSpec = new IngredientListSpecification(page, pageSize, ingredientTypeId, isActive, search);
        var countSpec = new IngredientCountSpecification(ingredientTypeId, isActive, search);

        var items = await _ingredientRepo.ListAsync(listSpec, ct);
        var totalCount = await _ingredientRepo.CountAsync(countSpec, ct);

        return new PaginatedResponse<IngredientDetailResponse>
        {
            Data = _mapper.Map<IReadOnlyList<IngredientDetailResponse>>(items),
            Meta = new PaginationMeta
            {
                Page = page,
                Take = pageSize,
                ItemCount = totalCount
            }
        };
    }

    public async Task<IngredientDetailResponse> CreateAsync(CreateIngredientRequest request, CancellationToken ct = default)
    {
        request.IngredientTypeId = (await _ingredientTypeRepo.GetRequiredByUuidAsync(request.IngredientTypeUuid, nameof(IngredientType), ct)).Id;

        if (request.DefaultUnitUuid.HasValue)
            request.DefaultUnitId = (await _unitRepo.GetRequiredByUuidAsync(request.DefaultUnitUuid.Value, nameof(Unit), ct)).Id;

        var ingredient = _factory.Create(request);

        await _ingredientRepo.InsertAsync(ingredient, ct);
        await _ingredientRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(ingredient.Id, ct))!;
    }

    public async Task<IngredientDetailResponse> UpdateAsync(Guid uuid, UpdateIngredientRequest request, CancellationToken ct = default)
    {
        var ingredient = await _ingredientRepo.GetRequiredByUuidAsync(uuid, nameof(Ingredient), ct);

        if (request.IngredientTypeUuid.HasValue)
            request.IngredientTypeId = (await _ingredientTypeRepo.GetRequiredByUuidAsync(request.IngredientTypeUuid.Value, nameof(IngredientType), ct)).Id;

        if (!request.ClearDefaultUnit && request.DefaultUnitUuid.HasValue)
            request.DefaultUnitId = (await _unitRepo.GetRequiredByUuidAsync(request.DefaultUnitUuid.Value, nameof(Unit), ct)).Id;

        _factory.Update(ingredient, request);

        _ingredientRepo.Update(ingredient);
        await _ingredientRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(ingredient.Id, ct))!;
    }

    public async Task<IngredientDetailResponse> ToggleActiveAsync(Guid uuid, CancellationToken ct = default)
    {
        var ingredient = await _ingredientRepo.GetRequiredByUuidAsync(uuid, nameof(Ingredient), ct);

        ingredient.IsActive = !ingredient.IsActive;

        _ingredientRepo.Update(ingredient);
        await _ingredientRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(ingredient.Id, ct))!;
    }

    public async Task DeleteAsync(Guid uuid, CancellationToken ct = default)
    {
        var ingredient = await _ingredientRepo.GetRequiredByUuidAsync(uuid, nameof(Ingredient), ct);

        var usedInRecipes = _recipeIngredientRepo.GetAll().Any(ri => ri.IngredientId == ingredient.Id);
        if (usedInRecipes)
            throw new FoodiyaBadRequestException("Cannot delete an Ingredient that is still used by recipes.");

        var images = _ingredientImageRepo.GetAll()
            .Where(image => image.IngredientId == ingredient.Id)
            .ToList();

        foreach (var image in images)
            _ingredientImageRepo.Delete(image);

        _ingredientRepo.Delete(ingredient);
        await _ingredientRepo.SaveAsync(ct);
    }

    private async Task<IngredientDetailResponse?> GetByIdInternalAsync(int id, CancellationToken ct)
    {
        var ingredient = await _ingredientRepo.GetSingleAsync(new IngredientByIdSpecification(id), ct);
        return ingredient is null ? null : _mapper.Map<IngredientDetailResponse>(ingredient);
    }

}
