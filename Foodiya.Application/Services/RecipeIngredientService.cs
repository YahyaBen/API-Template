using AutoMapper;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Application.DTOs.RecipeIngredient.Request;
using Foodiya.Application.DTOs.RecipeIngredient.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Application.Interfaces.Services;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using Foodiya.Domain.Specifications.RecipeIngredients;

namespace Foodiya.Application.Services;

public sealed class RecipeIngredientService : IRecipeIngredientService
{
    private readonly IRecipeIngredientRepository _recipeIngredientRepo;
    private readonly IGenericRepository<Recipe> _recipeRepo;
    private readonly IGenericRepository<Ingredient> _ingredientRepo;
    private readonly IGenericRepository<Unit> _unitRepo;
    private readonly IMapper _mapper;
    private readonly IRecipeIngredientFactory _factory;

    public RecipeIngredientService(
        IRecipeIngredientRepository recipeIngredientRepo,
        IGenericRepository<Recipe> recipeRepo,
        IGenericRepository<Ingredient> ingredientRepo,
        IGenericRepository<Unit> unitRepo,
        IMapper mapper,
        IRecipeIngredientFactory factory)
    {
        _recipeIngredientRepo = recipeIngredientRepo;
        _recipeRepo = recipeRepo;
        _ingredientRepo = ingredientRepo;
        _unitRepo = unitRepo;
        _mapper = mapper;
        _factory = factory;
    }

    public async Task<RecipeIngredientDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default)
    {
        var recipeIngredient = await _recipeIngredientRepo.GetByUuidAsync(uuid, ct);
        return recipeIngredient is null ? null : await GetByIdInternalAsync(recipeIngredient.RecipeId, recipeIngredient.IngredientId, ct);
    }

    public async Task<PaginatedResponse<RecipeIngredientDetailResponse>> ListAsync(
        int page,
        int pageSize,
        Guid? recipeUuid,
        Guid? ingredientUuid,
        Guid? unitUuid,
        string? search,
        CancellationToken ct = default)
    {
        var recipeId = recipeUuid.HasValue ? (await _recipeRepo.GetRequiredByUuidAsync(recipeUuid.Value, nameof(Recipe), ct)).Id : (int?)null;
        var ingredientId = ingredientUuid.HasValue ? (await _ingredientRepo.GetRequiredByUuidAsync(ingredientUuid.Value, nameof(Ingredient), ct)).Id : (int?)null;
        var unitId = unitUuid.HasValue ? (await _unitRepo.GetRequiredByUuidAsync(unitUuid.Value, nameof(Unit), ct)).Id : (int?)null;

        var listSpec = new RecipeIngredientListSpecification(page, pageSize, recipeId, ingredientId, unitId, search);
        var countSpec = new RecipeIngredientCountSpecification(recipeId, ingredientId, unitId, search);

        var items = await _recipeIngredientRepo.ListAsync(listSpec, ct);
        var totalCount = await _recipeIngredientRepo.CountAsync(countSpec, ct);

        return new PaginatedResponse<RecipeIngredientDetailResponse>
        {
            Data = _mapper.Map<IReadOnlyList<RecipeIngredientDetailResponse>>(items),
            Meta = new PaginationMeta
            {
                Page = page,
                Take = pageSize,
                ItemCount = totalCount
            }
        };
    }

    public async Task<RecipeIngredientDetailResponse> CreateAsync(CreateRecipeIngredientRequest request, CancellationToken ct = default)
    {
        request.RecipeId = (await _recipeRepo.GetRequiredByUuidAsync(request.RecipeUuid, nameof(Recipe), ct)).Id;
        request.IngredientId = (await _ingredientRepo.GetRequiredByUuidAsync(request.IngredientUuid, nameof(Ingredient), ct)).Id;
        await EnsureRecipeExistsAsync(request.RecipeId, ct);
        await EnsureIngredientExistsAsync(request.IngredientId, ct);

        if (request.UnitUuid.HasValue)
            request.UnitId = (await _unitRepo.GetRequiredByUuidAsync(request.UnitUuid.Value, nameof(Unit), ct)).Id;

        var existing = await _recipeIngredientRepo.GetSingleAsync(new RecipeIngredientByIdSpecification(request.RecipeId, request.IngredientId), ct);
        if (existing is not null)
            throw new FoodiyaValueAlreadyExistsException($"RecipeIngredient for Recipe ID {request.RecipeId} and Ingredient ID {request.IngredientId} already exists.");

        var recipeIngredient = _factory.Create(request);

        await _recipeIngredientRepo.InsertAsync(recipeIngredient, ct);
        await _recipeIngredientRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(recipeIngredient.RecipeId, recipeIngredient.IngredientId, ct))!;
    }

    public async Task<RecipeIngredientDetailResponse> UpdateAsync(Guid uuid, UpdateRecipeIngredientRequest request, CancellationToken ct = default)
    {
        var recipeIngredient = await _recipeIngredientRepo.GetRequiredByUuidAsync(uuid, nameof(RecipeIngredient), ct);

        await EnsureRecipeExistsAsync(recipeIngredient.RecipeId, ct);
        await EnsureIngredientExistsAsync(recipeIngredient.IngredientId, ct);

        if (!request.ClearUnitId && request.UnitUuid.HasValue)
            request.UnitId = (await _unitRepo.GetRequiredByUuidAsync(request.UnitUuid.Value, nameof(Unit), ct)).Id;

        _factory.Update(recipeIngredient, request);

        _recipeIngredientRepo.Update(recipeIngredient);
        await _recipeIngredientRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(recipeIngredient.RecipeId, recipeIngredient.IngredientId, ct))!;
    }

    public async Task DeleteAsync(Guid uuid, CancellationToken ct = default)
    {
        var recipeIngredient = await _recipeIngredientRepo.GetRequiredByUuidAsync(uuid, nameof(RecipeIngredient), ct);

        _recipeIngredientRepo.Delete(recipeIngredient);
        await _recipeIngredientRepo.SaveAsync(ct);
    }

    private async Task<RecipeIngredientDetailResponse?> GetByIdInternalAsync(int recipeId, int ingredientId, CancellationToken ct)
    {
        var recipeIngredient = await _recipeIngredientRepo.GetSingleAsync(new RecipeIngredientByIdSpecification(recipeId, ingredientId), ct);
        return recipeIngredient is null ? null : _mapper.Map<RecipeIngredientDetailResponse>(recipeIngredient);
    }

    private async Task EnsureRecipeExistsAsync(int recipeId, CancellationToken ct)
    {
        var recipe = await _recipeRepo.GetByIdAsync(recipeId, ct: ct)
            ?? throw new FoodiyaNotFoundException($"Recipe with ID {recipeId} not found.");

        if (recipe.DeletedAt is not null)
            throw new FoodiyaBadRequestException($"Recipe with ID {recipeId} is deleted and cannot have ingredients.");
    }

    private async Task EnsureIngredientExistsAsync(int ingredientId, CancellationToken ct)
    {
        _ = await _ingredientRepo.GetByIdAsync(ingredientId, ct: ct)
            ?? throw new FoodiyaNotFoundException($"Ingredient with ID {ingredientId} not found.");
    }

}
