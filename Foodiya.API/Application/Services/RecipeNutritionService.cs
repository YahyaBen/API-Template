using AutoMapper;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Application.DTOs.RecipeNutrition.Request;
using Foodiya.Application.DTOs.RecipeNutrition.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Application.Interfaces.Services;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using Foodiya.Domain.Specifications.RecipeNutritions;

namespace Foodiya.Application.Services;

public sealed class RecipeNutritionService : IRecipeNutritionService
{
    private readonly IRecipeNutritionRepository _recipeNutritionRepo;
    private readonly IGenericRepository<Recipe> _recipeRepo;
    private readonly IMapper _mapper;
    private readonly IRecipeNutritionFactory _factory;

    public RecipeNutritionService(
        IRecipeNutritionRepository recipeNutritionRepo,
        IGenericRepository<Recipe> recipeRepo,
        IMapper mapper,
        IRecipeNutritionFactory factory)
    {
        _recipeNutritionRepo = recipeNutritionRepo;
        _recipeRepo = recipeRepo;
        _mapper = mapper;
        _factory = factory;
    }

    public async Task<RecipeNutritionDetailResponse?> GetByIdAsync(Guid recipeUuid, CancellationToken ct = default)
    {
        var recipeId = (await _recipeRepo.GetRequiredByUuidAsync(recipeUuid, nameof(Recipe), ct)).Id;
        var recipeNutrition = await _recipeNutritionRepo.GetSingleAsync(new RecipeNutritionByIdSpecification(recipeId), ct);
        return recipeNutrition is null ? null : _mapper.Map<RecipeNutritionDetailResponse>(recipeNutrition);
    }

    public async Task<PaginatedResponse<RecipeNutritionDetailResponse>> ListAsync(
        int page,
        int pageSize,
        Guid? recipeUuid,
        string? search,
        CancellationToken ct = default)
    {
        var recipeId = recipeUuid.HasValue ? (await _recipeRepo.GetRequiredByUuidAsync(recipeUuid.Value, nameof(Recipe), ct)).Id : (int?)null;
        var listSpec = new RecipeNutritionListSpecification(page, pageSize, recipeId, search);
        var countSpec = new RecipeNutritionCountSpecification(recipeId, search);

        var items = await _recipeNutritionRepo.ListAsync(listSpec, ct);
        var totalCount = await _recipeNutritionRepo.CountAsync(countSpec, ct);

        return new PaginatedResponse<RecipeNutritionDetailResponse>
        {
            Data = _mapper.Map<IReadOnlyList<RecipeNutritionDetailResponse>>(items),
            Meta = new PaginationMeta
            {
                Page = page,
                Take = pageSize,
                ItemCount = totalCount
            }
        };
    }

    public async Task<RecipeNutritionDetailResponse> CreateAsync(CreateRecipeNutritionRequest request, CancellationToken ct = default)
    {
        request.RecipeId = (await _recipeRepo.GetRequiredByUuidAsync(request.RecipeUuid, nameof(Recipe), ct)).Id;
        await EnsureRecipeExistsAsync(request.RecipeId, ct);

        var existing = await _recipeNutritionRepo.GetByIdAsync(request.RecipeId, ct: ct);
        if (existing is not null)
            throw new FoodiyaValueAlreadyExistsException($"RecipeNutrition for Recipe ID {request.RecipeId} already exists.");

        var recipeNutrition = _factory.Create(request);

        await _recipeNutritionRepo.InsertAsync(recipeNutrition, ct);
        await _recipeNutritionRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(recipeNutrition.RecipeId, ct))!;
    }

    public async Task<RecipeNutritionDetailResponse> UpdateAsync(Guid recipeUuid, UpdateRecipeNutritionRequest request, CancellationToken ct = default)
    {
        var recipeId = (await _recipeRepo.GetRequiredByUuidAsync(recipeUuid, nameof(Recipe), ct)).Id;
        var recipeNutrition = await _recipeNutritionRepo.GetByIdAsync(recipeId, ct: ct)
            ?? throw new FoodiyaNotFoundException($"RecipeNutrition for Recipe UUID {recipeUuid} not found.");

        await EnsureRecipeExistsAsync(recipeId, ct);

        _factory.Update(recipeNutrition, request);

        _recipeNutritionRepo.Update(recipeNutrition);
        await _recipeNutritionRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(recipeNutrition.RecipeId, ct))!;
    }

    public async Task DeleteAsync(Guid recipeUuid, CancellationToken ct = default)
    {
        var recipeId = (await _recipeRepo.GetRequiredByUuidAsync(recipeUuid, nameof(Recipe), ct)).Id;
        var recipeNutrition = await _recipeNutritionRepo.GetByIdAsync(recipeId, ct: ct)
            ?? throw new FoodiyaNotFoundException($"RecipeNutrition for Recipe UUID {recipeUuid} not found.");

        _recipeNutritionRepo.Delete(recipeNutrition);
        await _recipeNutritionRepo.SaveAsync(ct);
    }

    private async Task<RecipeNutritionDetailResponse?> GetByIdInternalAsync(int recipeId, CancellationToken ct)
    {
        var recipeNutrition = await _recipeNutritionRepo.GetSingleAsync(new RecipeNutritionByIdSpecification(recipeId), ct);
        return recipeNutrition is null ? null : _mapper.Map<RecipeNutritionDetailResponse>(recipeNutrition);
    }

    private async Task EnsureRecipeExistsAsync(int recipeId, CancellationToken ct)
    {
        var recipe = await _recipeRepo.GetByIdAsync(recipeId, ct: ct)
            ?? throw new FoodiyaNotFoundException($"Recipe with ID {recipeId} not found.");

        if (recipe.DeletedAt is not null)
            throw new FoodiyaBadRequestException($"Recipe with ID {recipeId} is deleted and cannot have nutrition data.");
    }
}
