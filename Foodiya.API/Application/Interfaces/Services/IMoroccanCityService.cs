using Foodiya.Application.DTOs.MoroccanCity.Request;
using Foodiya.Application.DTOs.MoroccanCity.Response;
using Foodiya.Application.DTOs.Recipe.Response;

namespace Foodiya.Application.Interfaces.Services;

public interface IMoroccanCityService
{
    Task<MoroccanCityDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default);
    Task<PaginatedResponse<MoroccanCityDetailResponse>> ListAsync(int page, int pageSize, Guid? regionUuid, bool? isActive, string? search, CancellationToken ct = default);
    Task<MoroccanCityDetailResponse> CreateAsync(CreateMoroccanCityRequest request, CancellationToken ct = default);
    Task<MoroccanCityDetailResponse> UpdateAsync(Guid uuid, UpdateMoroccanCityRequest request, CancellationToken ct = default);
    Task<MoroccanCityDetailResponse> ToggleActiveAsync(Guid uuid, CancellationToken ct = default);
    Task DeleteAsync(Guid uuid, CancellationToken ct = default);
}
