using AutoMapper;
using Foodiya.Application.DTOs.FoodCategory.Request;
using Foodiya.Application.DTOs.FoodCategory.Response;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Application.Interfaces.Services;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using Foodiya.Domain.Specifications.FoodCategories;

namespace Foodiya.Application.Services;

public sealed class FoodCategoryService : IFoodCategoryService
{
    private readonly IFoodCategoryRepository _foodCategoryRepo;
    private readonly IGenericRepository<Recipe> _recipeRepo;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IFoodCategoryFactory _factory;

    public FoodCategoryService(
        IFoodCategoryRepository foodCategoryRepo,
        IGenericRepository<Recipe> recipeRepo,
        IMapper mapper,
        IDateTimeProvider dateTimeProvider,
        IFoodCategoryFactory factory)
    {
        _foodCategoryRepo = foodCategoryRepo;
        _recipeRepo = recipeRepo;
        _mapper = mapper;
        _dateTimeProvider = dateTimeProvider;
        _factory = factory;
    }

    public async Task<FoodCategoryDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default)
    {
        var foodCategory = await _foodCategoryRepo.GetByUuidAsync(uuid, ct);
        return foodCategory is null ? null : _mapper.Map<FoodCategoryDetailResponse>(foodCategory);
    }

    public async Task<PaginatedResponse<FoodCategoryDetailResponse>> ListAsync(
        int page,
        int pageSize,
        bool? isActive,
        string? search,
        CancellationToken ct = default)
    {
        var listSpec = new FoodCategoryListSpecification(page, pageSize, isActive, search);
        var countSpec = new FoodCategoryCountSpecification(isActive, search);

        var items = await _foodCategoryRepo.ListAsync(listSpec, ct);
        var totalCount = await _foodCategoryRepo.CountAsync(countSpec, ct);

        return new PaginatedResponse<FoodCategoryDetailResponse>
        {
            Data = _mapper.Map<IReadOnlyList<FoodCategoryDetailResponse>>(items),
            Meta = new PaginationMeta
            {
                Page = page,
                Take = pageSize,
                ItemCount = totalCount
            }
        };
    }

    public async Task<FoodCategoryDetailResponse> CreateAsync(CreateFoodCategoryRequest request, CancellationToken ct = default)
    {
        var foodCategory = _factory.Create(request);

        await _foodCategoryRepo.InsertAsync(foodCategory, ct);
        await _foodCategoryRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(foodCategory.Id, ct))!;
    }

    public async Task<FoodCategoryDetailResponse> UpdateAsync(Guid uuid, UpdateFoodCategoryRequest request, CancellationToken ct = default)
    {
        var foodCategory = await _foodCategoryRepo.GetRequiredByUuidAsync(uuid, nameof(FoodCategory), ct);

        _factory.Update(foodCategory, request, _dateTimeProvider.UtcNow);

        _foodCategoryRepo.Update(foodCategory);
        await _foodCategoryRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(foodCategory.Id, ct))!;
    }

    public async Task<FoodCategoryDetailResponse> ToggleActiveAsync(Guid uuid, CancellationToken ct = default)
    {
        var foodCategory = await _foodCategoryRepo.GetRequiredByUuidAsync(uuid, nameof(FoodCategory), ct);

        foodCategory.IsActive = !foodCategory.IsActive;
        foodCategory.DateModif = _dateTimeProvider.UtcNow;

        _foodCategoryRepo.Update(foodCategory);
        await _foodCategoryRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(foodCategory.Id, ct))!;
    }

    public async Task DeleteAsync(Guid uuid, CancellationToken ct = default)
    {
        var foodCategory = await _foodCategoryRepo.GetRequiredByUuidAsync(uuid, nameof(FoodCategory), ct);

        var hasRecipes = _recipeRepo.GetAll().Any(recipe => recipe.FoodCategories.Any(category => category.Id == foodCategory.Id));
        if (hasRecipes)
            throw new FoodiyaBadRequestException("Cannot delete a FoodCategory that is still used by recipes.");

        _foodCategoryRepo.Delete(foodCategory);
        await _foodCategoryRepo.SaveAsync(ct);
    }

    private async Task<FoodCategoryDetailResponse?> GetByIdInternalAsync(int id, CancellationToken ct)
    {
        var foodCategory = await _foodCategoryRepo.GetSingleAsync(new FoodCategoryByIdSpecification(id), ct);
        return foodCategory is null ? null : _mapper.Map<FoodCategoryDetailResponse>(foodCategory);
    }
}
