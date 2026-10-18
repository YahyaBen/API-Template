using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Application.DTOs.Unit.Request;
using Foodiya.Application.DTOs.Unit.Response;

namespace Foodiya.Application.Interfaces.Services;

public interface IUnitService
{
    Task<UnitDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default);
    Task<PaginatedResponse<UnitDetailResponse>> ListAsync(int page, int pageSize, bool? isActive, string? search, CancellationToken ct = default);
    Task<UnitDetailResponse> CreateAsync(CreateUnitRequest request, CancellationToken ct = default);
    Task<UnitDetailResponse> UpdateAsync(Guid uuid, UpdateUnitRequest request, CancellationToken ct = default);
    Task<UnitDetailResponse> ToggleActiveAsync(Guid uuid, CancellationToken ct = default);
    Task DeleteAsync(Guid uuid, CancellationToken ct = default);
}
