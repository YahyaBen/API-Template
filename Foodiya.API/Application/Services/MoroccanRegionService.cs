using AutoMapper;
using Foodiya.Application.DTOs.MoroccanRegion.Request;
using Foodiya.Application.DTOs.MoroccanRegion.Response;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Application.Interfaces.Services;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using Foodiya.Domain.Specifications.MoroccanRegions;

namespace Foodiya.Application.Services;

public sealed class MoroccanRegionService : IMoroccanRegionService
{
    private readonly IMoroccanRegionRepository _moroccanRegionRepo;
    private readonly IMoroccanCityRepository _moroccanCityRepo;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly IMoroccanRegionFactory _factory;

    public MoroccanRegionService(
        IMoroccanRegionRepository moroccanRegionRepo,
        IMoroccanCityRepository moroccanCityRepo,
        IMapper mapper,
        IDateTimeProvider dateTimeProvider,
        IMoroccanRegionFactory factory)
    {
        _moroccanRegionRepo = moroccanRegionRepo;
        _moroccanCityRepo = moroccanCityRepo;
        _mapper = mapper;
        _dateTimeProvider = dateTimeProvider;
        _factory = factory;
    }

    public async Task<MoroccanRegionDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default)
    {
        var region = await _moroccanRegionRepo.GetByUuidAsync(uuid, ct);
        return region is null ? null : _mapper.Map<MoroccanRegionDetailResponse>(region);
    }

    public async Task<PaginatedResponse<MoroccanRegionDetailResponse>> ListAsync(
        int page,
        int pageSize,
        bool? isActive,
        string? search,
        CancellationToken ct = default)
    {
        var listSpec = new MoroccanRegionListSpecification(page, pageSize, isActive, search);
        var countSpec = new MoroccanRegionCountSpecification(isActive, search);

        var items = await _moroccanRegionRepo.ListAsync(listSpec, ct);
        var totalCount = await _moroccanRegionRepo.CountAsync(countSpec, ct);

        return new PaginatedResponse<MoroccanRegionDetailResponse>
        {
            Data = _mapper.Map<IReadOnlyList<MoroccanRegionDetailResponse>>(items),
            Meta = new PaginationMeta
            {
                Page = page,
                Take = pageSize,
                ItemCount = totalCount
            }
        };
    }

    public async Task<MoroccanRegionDetailResponse> CreateAsync(CreateMoroccanRegionRequest request, CancellationToken ct = default)
    {
        var region = _factory.Create(request);

        await _moroccanRegionRepo.InsertAsync(region, ct);
        await _moroccanRegionRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(region.Id, ct))!;
    }

    public async Task<MoroccanRegionDetailResponse> UpdateAsync(Guid uuid, UpdateMoroccanRegionRequest request, CancellationToken ct = default)
    {
        var region = await _moroccanRegionRepo.GetRequiredByUuidAsync(uuid, nameof(MoroccanRegion), ct);

        _factory.Update(region, request, _dateTimeProvider.UtcNow);

        _moroccanRegionRepo.Update(region);
        await _moroccanRegionRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(region.Id, ct))!;
    }

    public async Task<MoroccanRegionDetailResponse> ToggleActiveAsync(Guid uuid, CancellationToken ct = default)
    {
        var region = await _moroccanRegionRepo.GetRequiredByUuidAsync(uuid, nameof(MoroccanRegion), ct);

        region.IsActive = !region.IsActive;
        region.DateModif = _dateTimeProvider.UtcNow;

        _moroccanRegionRepo.Update(region);
        await _moroccanRegionRepo.SaveAsync(ct);

        return (await GetByIdInternalAsync(region.Id, ct))!;
    }

    public async Task DeleteAsync(Guid uuid, CancellationToken ct = default)
    {
        var region = await _moroccanRegionRepo.GetRequiredByUuidAsync(uuid, nameof(MoroccanRegion), ct);

        var hasCities = _moroccanCityRepo.GetAll().Any(city => city.RegionId == region.Id);
        if (hasCities)
            throw new FoodiyaBadRequestException("Cannot delete a MoroccanRegion that is still used by Moroccan cities.");

        _moroccanRegionRepo.Delete(region);
        await _moroccanRegionRepo.SaveAsync(ct);
    }

    private async Task<MoroccanRegionDetailResponse?> GetByIdInternalAsync(int id, CancellationToken ct)
    {
        var region = await _moroccanRegionRepo.GetSingleAsync(new MoroccanRegionByIdSpecification(id), ct);
        return region is null ? null : _mapper.Map<MoroccanRegionDetailResponse>(region);
    }
}
