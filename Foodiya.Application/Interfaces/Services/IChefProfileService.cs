using Foodiya.Application.DTOs.ChefProfile.Request;
using Foodiya.Application.DTOs.ChefProfile.Response;
using Foodiya.Application.DTOs.Common;
using Foodiya.Application.DTOs.Recipe.Response;

namespace Foodiya.Application.Interfaces.Services;

public interface IChefProfileService
{
    Task<ChefProfileDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default);
    Task<PaginatedResponse<ChefProfileDetailResponse>> ListAsync(int page, int pageSize, bool? isVerified, string? search, CancellationToken ct = default);
    Task<ChefProfileDetailResponse> CreateAsync(Guid userUuid, CreateChefProfileRequest request, CancellationToken ct = default);
    Task<ChefProfileDetailResponse> CreateSelfAsync(Guid identityUserId, CreateChefProfileRequest request, CancellationToken ct = default);
    Task<ChefProfileDetailResponse> UpdateAsync(Guid uuid, UpdateChefProfileRequest request, CancellationToken ct = default);
    Task<ChefProfileDetailResponse> ToggleVerifiedAsync(Guid uuid, CancellationToken ct = default);
    Task DeleteAsync(Guid uuid, SoftDeleteRequest request, CancellationToken ct = default);
}
