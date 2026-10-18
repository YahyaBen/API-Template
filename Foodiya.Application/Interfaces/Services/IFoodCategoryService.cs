using Foodiya.Application.DTOs.FoodCategory.Request;
using Foodiya.Application.DTOs.FoodCategory.Response;
using Foodiya.Application.DTOs.Recipe.Response;

namespace Foodiya.Application.Interfaces.Services;

public interface IFoodCategoryService
{
    Task<FoodCategoryDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default);
    Task<PaginatedResponse<FoodCategoryDetailResponse>> ListAsync(int page, int pageSize, bool? isActive, string? search, CancellationToken ct = default);
    Task<FoodCategoryDetailResponse> CreateAsync(CreateFoodCategoryRequest request, CancellationToken ct = default);
    Task<FoodCategoryDetailResponse> UpdateAsync(Guid uuid, UpdateFoodCategoryRequest request, CancellationToken ct = default);
    Task<FoodCategoryDetailResponse> ToggleActiveAsync(Guid uuid, CancellationToken ct = default);
    Task DeleteAsync(Guid uuid, CancellationToken ct = default);
}
