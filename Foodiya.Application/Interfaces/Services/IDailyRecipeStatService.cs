using Foodiya.Application.DTOs.DailyRecipeStat.Response;
using Foodiya.Application.DTOs.Recipe.Response;

namespace Foodiya.Application.Interfaces.Services;

public interface IDailyRecipeStatService
{
    Task<PaginatedResponse<DailyRecipeStatDetailResponse>> ListAsync(int page, int pageSize, CancellationToken ct = default);
    Task<DailyRecipeStatDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default);
    Task<DailyRecipeStatDetailResponse?> GetTodayAsync(CancellationToken ct = default);
    Task<DailyRecipeStatDetailResponse?> GetLastAsync(CancellationToken ct = default);
}
