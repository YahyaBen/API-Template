using AutoMapper;
using Foodiya.Application.DTOs.Cuisine.Request;
using Foodiya.Application.DTOs.Cuisine.Response;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Application.Interfaces.Services;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using Foodiya.Domain.Specifications.Cuisines;

namespace Foodiya.Application.Services;

public sealed class CuisineService : ICuisineService
{
    private readonly ICuisineRepository _cuisineRepo;
    private readonly IGenericRepository<Recipe> _recipeRepo;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ICuisineFactory _factory;

    public CuisineService(
        ICuisineRepository cuisineRepo,
        IGenericRepository<Recipe> recipeRepo,
        IMapper mapper,
        IDateTimeProvider dateTimeProvider,
        ICuisineFactory factory)
    {
        _cuisineRepo = cuisineRepo;
        _recipeRepo = recipeRepo;
        _mapper = mapper;
        _dateTimeProvider = dateTimeProvider;
        _factory = factory;
    }

    public async Task<CuisineDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default)
    {
        var cuisine = await _cuisineRepo.GetByUuidAsync(uuid, ct);
        return cuisine is null ? null : _mapper.Map<CuisineDetailResponse>(cuisine);
    }

    public async Task<PaginatedResponse<CuisineDetailResponse>> ListAsync(
        int page,
        int pageSize,
        bool? isActive,
        string? search,
        CancellationToken ct = default)
    {
        var listSpec = new CuisineListSpecification(page, pageSize, isActive, search);
        var countSpec = new CuisineCountSpecification(isActive, search);

        var items = await _cuisineRepo.ListAsync(listSpec, ct);
        var totalCount = await _cuisineRepo.CountAsync(countSpec, ct);

        return new PaginatedResponse<CuisineDetailResponse>
        {
            Data = _mapper.Map<IReadOnlyList<CuisineDetailResponse>>(items),
            Meta = new PaginationMeta
            {
                Page = page,
                Take = pageSize,
                ItemCount = totalCount
            }
        };
    }

    public async Task<CuisineDetailResponse> CreateAsync(CreateCuisineRequest request, CancellationToken ct = default)
    {
        var cuisine = _factory.Create(request);

        await _cuisineRepo.InsertAsync(cuisine, ct);
        await _cuisineRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(cuisine.Id, ct))!;
    }

    public async Task<CuisineDetailResponse> UpdateAsync(Guid uuid, UpdateCuisineRequest request, CancellationToken ct = default)
    {
        var cuisine = await _cuisineRepo.GetRequiredByUuidAsync(uuid, nameof(Cuisine), ct);

        _factory.Update(cuisine, request, _dateTimeProvider.UtcNow);

        _cuisineRepo.Update(cuisine);
        await _cuisineRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(cuisine.Id, ct))!;
    }

    public async Task<CuisineDetailResponse> ToggleActiveAsync(Guid uuid, CancellationToken ct = default)
    {
        var cuisine = await _cuisineRepo.GetRequiredByUuidAsync(uuid, nameof(Cuisine), ct);

        cuisine.IsActive = !cuisine.IsActive;
        cuisine.DateModif = _dateTimeProvider.UtcNow;

        _cuisineRepo.Update(cuisine);
        await _cuisineRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(cuisine.Id, ct))!;
    }

    public async Task DeleteAsync(Guid uuid, CancellationToken ct = default)
    {
        var cuisine = await _cuisineRepo.GetRequiredByUuidAsync(uuid, nameof(Cuisine), ct);

        var hasRecipes = _recipeRepo.GetAll().Any(recipe => recipe.CuisineId == cuisine.Id);
        if (hasRecipes)
            throw new FoodiyaBadRequestException("Cannot delete a Cuisine that is still used by recipes.");

        _cuisineRepo.Delete(cuisine);
        await _cuisineRepo.SaveAsync(ct);
    }

    private async Task<CuisineDetailResponse?> GetByIdInternalAsync(int id, CancellationToken ct)
    {
        var cuisine = await _cuisineRepo.GetSingleAsync(new CuisineByIdSpecification(id), ct);
        return cuisine is null ? null : _mapper.Map<CuisineDetailResponse>(cuisine);
    }
}
