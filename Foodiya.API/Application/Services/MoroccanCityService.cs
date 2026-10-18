using AutoMapper;
using Foodiya.Application.DTOs.MoroccanCity.Request;
using Foodiya.Application.DTOs.MoroccanCity.Response;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Application.Interfaces.Services;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using Foodiya.Domain.Specifications.MoroccanCities;

namespace Foodiya.Application.Services;

public sealed class MoroccanCityService : IMoroccanCityService
{
    private readonly IMoroccanCityRepository _moroccanCityRepo;
    private readonly IGenericRepository<MoroccanRegion> _moroccanRegionRepo;
    private readonly IGenericRepository<Recipe> _recipeRepo;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IMoroccanCityFactory _factory;

    public MoroccanCityService(
        IMoroccanCityRepository moroccanCityRepo,
        IGenericRepository<MoroccanRegion> moroccanRegionRepo,
        IGenericRepository<Recipe> recipeRepo,
        IMapper mapper,
        IDateTimeProvider dateTimeProvider,
        IMoroccanCityFactory factory)
    {
        _moroccanCityRepo = moroccanCityRepo;
        _moroccanRegionRepo = moroccanRegionRepo;
        _recipeRepo = recipeRepo;
        _mapper = mapper;
        _dateTimeProvider = dateTimeProvider;
        _factory = factory;
    }

    public async Task<MoroccanCityDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default)
    {
        var city = await _moroccanCityRepo.GetByUuidAsync(uuid, ct);
        return city is null ? null : await GetByIdInternalAsync(city.Id, ct);
    }

    public async Task<PaginatedResponse<MoroccanCityDetailResponse>> ListAsync(
        int page,
        int pageSize,
        Guid? regionUuid,
        bool? isActive,
        string? search,
        CancellationToken ct = default)
    {
        var regionId = regionUuid.HasValue
            ? (await _moroccanRegionRepo.GetRequiredByUuidAsync(regionUuid.Value, nameof(MoroccanRegion), ct)).Id
            : (int?)null;

        var listSpec = new MoroccanCityListSpecification(page, pageSize, regionId, isActive, search);
        var countSpec = new MoroccanCityCountSpecification(regionId, isActive, search);

        var items = await _moroccanCityRepo.ListAsync(listSpec, ct);
        var totalCount = await _moroccanCityRepo.CountAsync(countSpec, ct);

        return new PaginatedResponse<MoroccanCityDetailResponse>
        {
            Data = _mapper.Map<IReadOnlyList<MoroccanCityDetailResponse>>(items),
            Meta = new PaginationMeta
            {
                Page = page,
                Take = pageSize,
                ItemCount = totalCount
            }
        };
    }

    public async Task<MoroccanCityDetailResponse> CreateAsync(CreateMoroccanCityRequest request, CancellationToken ct = default)
    {
        request.RegionId = (await _moroccanRegionRepo.GetRequiredByUuidAsync(request.RegionUuid, nameof(MoroccanRegion), ct)).Id;

        var city = _factory.Create(request);

        await _moroccanCityRepo.InsertAsync(city, ct);
        await _moroccanCityRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(city.Id, ct))!;
    }

    public async Task<MoroccanCityDetailResponse> UpdateAsync(Guid uuid, UpdateMoroccanCityRequest request, CancellationToken ct = default)
    {
        var city = await _moroccanCityRepo.GetRequiredByUuidAsync(uuid, nameof(MoroccanCity), ct);

        if (request.RegionUuid.HasValue)
            request.RegionId = (await _moroccanRegionRepo.GetRequiredByUuidAsync(request.RegionUuid.Value, nameof(MoroccanRegion), ct)).Id;

        _factory.Update(city, request, _dateTimeProvider.UtcNow);

        _moroccanCityRepo.Update(city);
        await _moroccanCityRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(city.Id, ct))!;
    }

    public async Task<MoroccanCityDetailResponse> ToggleActiveAsync(Guid uuid, CancellationToken ct = default)
    {
        var city = await _moroccanCityRepo.GetRequiredByUuidAsync(uuid, nameof(MoroccanCity), ct);

        city.IsActive = !city.IsActive;
        city.DateModif = _dateTimeProvider.UtcNow;

        _moroccanCityRepo.Update(city);
        await _moroccanCityRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(city.Id, ct))!;
    }

    public async Task DeleteAsync(Guid uuid, CancellationToken ct = default)
    {
        var city = await _moroccanCityRepo.GetRequiredByUuidAsync(uuid, nameof(MoroccanCity), ct);

        var hasRecipes = _recipeRepo.GetAll().Any(recipe => recipe.CityId == city.Id);
        if (hasRecipes)
            throw new FoodiyaBadRequestException("Cannot delete a MoroccanCity that is still used by recipes.");

        _moroccanCityRepo.Delete(city);
        await _moroccanCityRepo.SaveAsync(ct);
    }

    private async Task<MoroccanCityDetailResponse?> GetByIdInternalAsync(int id, CancellationToken ct)
    {
        var city = await _moroccanCityRepo.GetSingleAsync(new MoroccanCityByIdSpecification(id), ct);
        return city is null ? null : _mapper.Map<MoroccanCityDetailResponse>(city);
    }

    private async Task EnsureRegionExistsAsync(int regionId, CancellationToken ct)
    {
        _ = await _moroccanRegionRepo.GetByIdAsync(regionId, ct: ct)
            ?? throw new FoodiyaNotFoundException($"MoroccanRegion with ID {regionId} not found.");
    }
}
