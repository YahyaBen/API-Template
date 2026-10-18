using Foodiya.Application.DTOs.PresetAvatarImage.Request;
using Foodiya.Application.DTOs.PresetAvatarImage.Response;
using Foodiya.Application.DTOs.Recipe.Response;

namespace Foodiya.Application.Interfaces.Services;

public interface IPresetAvatarImageService
{
    Task<PresetAvatarImageDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default);
    Task<PaginatedResponse<PresetAvatarImageDetailResponse>> ListAsync(int page, int pageSize, bool? isActive, string? search, CancellationToken ct = default);
    Task<PresetAvatarImageDetailResponse> CreateAsync(CreatePresetAvatarImageRequest request, CancellationToken ct = default);
    Task<PresetAvatarImageDetailResponse> UpdateAsync(Guid uuid, UpdatePresetAvatarImageRequest request, CancellationToken ct = default);
    Task<PresetAvatarImageDetailResponse> ToggleActiveAsync(Guid uuid, CancellationToken ct = default);
    Task DeleteAsync(Guid uuid, CancellationToken ct = default);
}
