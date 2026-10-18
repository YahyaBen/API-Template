using AutoMapper;
using Foodiya.Application.DTOs.DailyRecipeStat.Response;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Application.Interfaces.Services;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using Foodiya.Domain.Specifications.DailyRecipeStats;

namespace Foodiya.Application.Services;

public sealed class DailyRecipeStatService : IDailyRecipeStatService
{
    private readonly IGenericRepository<DailyRecipeStat> _repo;
    private readonly IMapper _mapper;
    private readonly IDateTimeProvider _dateTimeProvider;

    public DailyRecipeStatService(
        IGenericRepository<DailyRecipeStat> repo,
        IMapper mapper,
        IDateTimeProvider dateTimeProvider)
    {
        _repo = repo;
        _mapper = mapper;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task<PaginatedResponse<DailyRecipeStatDetailResponse>> ListAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var listSpec = new DailyRecipeStatListSpecification(page, pageSize);
        var countSpec = new DailyRecipeStatCountSpecification();

        var items = await _repo.ListAsync(listSpec, ct);
        var totalCount = await _repo.CountAsync(countSpec, ct);

        return new PaginatedResponse<DailyRecipeStatDetailResponse>
        {
            Data = _mapper.Map<IReadOnlyList<DailyRecipeStatDetailResponse>>(items),
            Meta = new PaginationMeta
            {
                Page = page,
                Take = pageSize,
                ItemCount = totalCount
            }
        };
    }

    public async Task<DailyRecipeStatDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default)
    {
        var entity = await _repo.GetByUuidAsync(uuid, ct);
        return entity is null ? null : _mapper.Map<DailyRecipeStatDetailResponse>(entity);
    }

    public async Task<DailyRecipeStatDetailResponse?> GetTodayAsync(CancellationToken ct = default)
    {
        var today = _dateTimeProvider.UtcNow.Date;
        var spec = new DailyRecipeStatByDateSpecification(today);
        var entity = await _repo.GetSingleAsync(spec, ct);
        return entity is null ? null : _mapper.Map<DailyRecipeStatDetailResponse>(entity);
    }

    public async Task<DailyRecipeStatDetailResponse?> GetLastAsync(CancellationToken ct = default)
    {
        var spec = new DailyRecipeStatLastSpecification();
        var items = await _repo.ListAsync(spec, ct);
        var entity = items.FirstOrDefault();
        return entity is null ? null : _mapper.Map<DailyRecipeStatDetailResponse>(entity);
    }
}
