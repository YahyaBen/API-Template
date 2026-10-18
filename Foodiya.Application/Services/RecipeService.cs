using AutoMapper;
using Foodiya.Application.DTOs.Common;
using Foodiya.Application.DTOs.Recipe.Request;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Application.Interfaces.Services;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using Foodiya.Domain.Specifications.Recipes;

namespace Foodiya.Application.Services;

public sealed class RecipeService : IRecipeService
{
    private readonly IGenericRepository<Recipe> _recipeRepo;
    private readonly IGenericRepository<FoodCategory> _categoryRepo;
    private readonly IGenericRepository<Ingredient> _ingredientRepo;
    private readonly IGenericRepository<ChefProfile> _chefRepo;
    private readonly IGenericRepository<Difficulty> _difficultyRepo;
    private readonly IGenericRepository<Cuisine> _cuisineRepo;
    private readonly IGenericRepository<MoroccanCity> _cityRepo;
    private readonly IGenericRepository<Unit> _unitRepo;
    private readonly IGenericRepository<AppUser> _userRepo;
    private readonly IMapper _mapper;
    private readonly IRecipeFactory _factory;
    private readonly IDateTimeProvider _dateTimeProvider;

    public RecipeService(
        IGenericRepository<Recipe> recipeRepo,
        IGenericRepository<FoodCategory> categoryRepo,
        IGenericRepository<Ingredient> ingredientRepo,
        IGenericRepository<ChefProfile> chefRepo,
        IGenericRepository<Difficulty> difficultyRepo,
        IGenericRepository<Cuisine> cuisineRepo,
        IGenericRepository<MoroccanCity> cityRepo,
        IGenericRepository<Unit> unitRepo,
        IGenericRepository<AppUser> userRepo,
        IMapper mapper,
        IRecipeFactory factory,
        IDateTimeProvider dateTimeProvider)
    {
        _recipeRepo = recipeRepo;
        _categoryRepo = categoryRepo;
        _ingredientRepo = ingredientRepo;
        _chefRepo = chefRepo;
        _difficultyRepo = difficultyRepo;
        _cuisineRepo = cuisineRepo;
        _cityRepo = cityRepo;
        _unitRepo = unitRepo;
        _userRepo = userRepo;
        _mapper = mapper;
        _factory = factory;
        _dateTimeProvider = dateTimeProvider;
    }

    // ─── Queries ────────────────────────────────────────────────

    public async Task<RecipeDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default)
    {
        var recipe = await _recipeRepo.GetByUuidAsync(uuid, ct);
        return recipe is null ? null : await GetByIdInternalAsync(recipe.Id, ct);
    }

    public async Task<PaginatedResponse<RecipeDetailResponse>> ListAsync(
        int page, int pageSize,
        Guid? cuisineUuid, Guid? difficultyUuid, Guid? categoryUuid, string? search,
        CancellationToken ct = default)
    {
        var cuisineId = cuisineUuid.HasValue
            ? (await _cuisineRepo.GetRequiredByUuidAsync(cuisineUuid.Value, nameof(Cuisine), ct)).Id
            : (int?)null;
        var difficultyId = difficultyUuid.HasValue
            ? (await _difficultyRepo.GetRequiredByUuidAsync(difficultyUuid.Value, nameof(Difficulty), ct)).Id
            : (int?)null;
        var categoryId = categoryUuid.HasValue
            ? (await _categoryRepo.GetRequiredByUuidAsync(categoryUuid.Value, nameof(FoodCategory), ct)).Id
            : (int?)null;

        var listSpec = new RecipeListSpecification(page, pageSize, cuisineId, difficultyId, categoryId, search);
        var countSpec = new RecipeCountSpecification(cuisineId, difficultyId, categoryId, search);

        var items = await _recipeRepo.ListAsync(listSpec, ct);
        var totalCount = await _recipeRepo.CountAsync(countSpec, ct);

        return new PaginatedResponse<RecipeDetailResponse>
        {
            Data = _mapper.Map<IReadOnlyList<RecipeDetailResponse>>(items),
            Meta = new PaginationMeta
            {
                Page = page,
                Take = pageSize,
                ItemCount = totalCount
            }
        };
    }

    // ─── Commands ───────────────────────────────────────────────

    public async Task<RecipeDetailResponse> CreateAsync(Guid chefUuid, CreateRecipeRequest request, CancellationToken ct = default)
    {
        var chef = await _chefRepo.GetRequiredByUuidAsync(chefUuid, nameof(ChefProfile), ct);
        await ResolveRecipeRequestAsync(request, ct);

        var recipe = _factory.Create(chef.Id, request);

        await ResolveCategoriesAsync(recipe, request.FoodCategoryIds, ct);
        await ResolveIngredientsAsync(recipe, ct);

        await _recipeRepo.InsertAsync(recipe, ct);
        await _recipeRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(recipe.Id, ct))!;
    }

    public async Task<RecipeDetailResponse> UpdateAsync(Guid uuid, UpdateRecipeRequest request, CancellationToken ct = default)
    {
        var lookup = await _recipeRepo.GetRequiredByUuidAsync(uuid, nameof(Recipe), ct);
        var recipe = await _recipeRepo.GetSingleAsync(new RecipeForUpdateSpecification(lookup.Id), ct)
            ?? throw new FoodiyaNotFoundException($"Recipe with UUID {uuid} not found.");

        await ResolveRecipeRequestAsync(request, ct);
        _factory.Update(recipe, request);

        if (request.FoodCategoryIds is not null)
            await ResolveCategoriesAsync(recipe, request.FoodCategoryIds, ct);

        if (request.Ingredients is not null)
            await ResolveIngredientsAsync(recipe, ct);

        _recipeRepo.Update(recipe);
        await _recipeRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(recipe.Id, ct))!;
    }

    public async Task<RecipeDetailResponse> ToggleActiveAsync(Guid uuid, CancellationToken ct = default)
    {
        var recipe = await _recipeRepo.GetRequiredByUuidAsync(uuid, nameof(Recipe), ct);

        recipe.IsActive = !recipe.IsActive;
        recipe.DateModif = _dateTimeProvider.UtcNow;

        _recipeRepo.Update(recipe);
        await _recipeRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(recipe.Id, ct))!;
    }

    public async Task DeleteAsync(Guid uuid, SoftDeleteRequest request, CancellationToken ct = default)
    {
        var recipe = await _recipeRepo.GetRequiredByUuidAsync(uuid, nameof(Recipe), ct);

        if (recipe.DeletedAt is not null)
            throw new FoodiyaBadRequestException($"Recipe with UUID {uuid} is already deleted.");

        request.DeletedByUserId = (await _userRepo.GetRequiredByUuidAsync(request.DeletedByUserUuid, nameof(AppUser), ct)).Id;
        recipe.DeletedAt = _dateTimeProvider.UtcNow;
        recipe.DeletedByUserId = request.DeletedByUserId;
        recipe.DeleteReason = request.DeleteReason;
        recipe.DateModif = _dateTimeProvider.UtcNow;

        _recipeRepo.Update(recipe);
        await _recipeRepo.SaveAsync(ct);
    }

    // ─── Private helpers ────────────────────────────────────────

    private async Task<RecipeDetailResponse?> GetByIdInternalAsync(int id, CancellationToken ct)
    {
        var recipe = await _recipeRepo.GetSingleAsync(new RecipeByIdSpecification(id, includeInactive: true), ct);
        return recipe is null ? null : _mapper.Map<RecipeDetailResponse>(recipe);
    }

    private async Task ResolveCategoriesAsync(Recipe recipe, ICollection<int> categoryIds, CancellationToken ct)
    {
        recipe.FoodCategories.Clear();
        foreach (var catId in categoryIds)
        {
            var category = await _categoryRepo.GetByIdAsync(catId, ct: ct)
                ?? throw new FoodiyaNotFoundException($"FoodCategory with ID {catId} not found.");
            recipe.FoodCategories.Add(category);
        }
    }

    /// <summary>
    /// Resolves each RecipeIngredient's Ingredient navigation from the DB
    /// so EF's change tracker can properly handle the composite PK/FK.
    /// </summary>
    private async Task ResolveIngredientsAsync(Recipe recipe, CancellationToken ct)
    {
        foreach (var ri in recipe.RecipeIngredients)
        {
            ri.Ingredient = await _ingredientRepo.GetByIdAsync(ri.IngredientId, ct: ct)
                ?? throw new FoodiyaNotFoundException($"Ingredient with ID {ri.IngredientId} not found.");
        }
    }

    private async Task ResolveRecipeRequestAsync(CreateRecipeRequest request, CancellationToken ct)
    {
        request.DifficultyId = (await _difficultyRepo.GetRequiredByUuidAsync(request.DifficultyUuid, nameof(Difficulty), ct)).Id;
        request.CuisineId = request.CuisineUuid.HasValue
            ? (await _cuisineRepo.GetRequiredByUuidAsync(request.CuisineUuid.Value, nameof(Cuisine), ct)).Id
            : null;
        request.CityId = request.CityUuid.HasValue
            ? (await _cityRepo.GetRequiredByUuidAsync(request.CityUuid.Value, nameof(MoroccanCity), ct)).Id
            : null;
        request.FoodCategoryIds = await ResolveCategoryIdsAsync(request.FoodCategoryUuids, ct);
        await ResolveIngredientItemsAsync(request.Ingredients, ct);
    }

    private async Task ResolveRecipeRequestAsync(UpdateRecipeRequest request, CancellationToken ct)
    {
        request.DifficultyId = request.DifficultyUuid.HasValue
            ? (await _difficultyRepo.GetRequiredByUuidAsync(request.DifficultyUuid.Value, nameof(Difficulty), ct)).Id
            : null;
        request.CuisineId = request.CuisineUuid.HasValue
            ? (await _cuisineRepo.GetRequiredByUuidAsync(request.CuisineUuid.Value, nameof(Cuisine), ct)).Id
            : null;
        request.CityId = request.CityUuid.HasValue
            ? (await _cityRepo.GetRequiredByUuidAsync(request.CityUuid.Value, nameof(MoroccanCity), ct)).Id
            : null;
        request.FoodCategoryIds = request.FoodCategoryUuids is null
            ? null
            : await ResolveCategoryIdsAsync(request.FoodCategoryUuids, ct);

        if (request.Ingredients is not null)
            await ResolveIngredientItemsAsync(request.Ingredients, ct);
    }

    private async Task<ICollection<int>> ResolveCategoryIdsAsync(ICollection<Guid> categoryUuids, CancellationToken ct)
    {
        var ids = new List<int>(categoryUuids.Count);
        foreach (var categoryUuid in categoryUuids)
            ids.Add((await _categoryRepo.GetRequiredByUuidAsync(categoryUuid, nameof(FoodCategory), ct)).Id);

        return ids;
    }

    private async Task ResolveIngredientItemsAsync(IEnumerable<CreateRecipeIngredientItem> items, CancellationToken ct)
    {
        foreach (var item in items)
        {
            item.IngredientId = (await _ingredientRepo.GetRequiredByUuidAsync(item.IngredientUuid, nameof(Ingredient), ct)).Id;
            item.UnitId = item.UnitUuid.HasValue
                ? (await _unitRepo.GetRequiredByUuidAsync(item.UnitUuid.Value, nameof(Unit), ct)).Id
                : null;
        }
    }
}
