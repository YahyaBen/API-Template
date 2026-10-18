using Foodiya.Application.DTOs.Difficulty.Request;
using Foodiya.Application.DTOs.Difficulty.Response;
using Foodiya.Application.DTOs.Recipe.Response;

namespace Foodiya.Application.Interfaces.Services;

public interface IDifficultyService
{
    Task<DifficultyDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default);
    Task<PaginatedResponse<DifficultyDetailResponse>> ListAsync(int page, int pageSize, bool? isActive, string? search, CancellationToken ct = default);
    Task<DifficultyDetailResponse> CreateAsync(CreateDifficultyRequest request, CancellationToken ct = default);
    Task<DifficultyDetailResponse> UpdateAsync(Guid uuid, UpdateDifficultyRequest request, CancellationToken ct = default);
    Task<DifficultyDetailResponse> ToggleActiveAsync(Guid uuid, CancellationToken ct = default);
    Task DeleteAsync(Guid uuid, CancellationToken ct = default);
}
