using Foodiya.Application.DTOs.Common;
using Foodiya.Application.DTOs.Recipe.Request;
using Foodiya.Application.DTOs.Recipe.Response;

namespace Foodiya.Application.Interfaces.Services;

public interface IRecipeService
{
    Task<RecipeDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default);
    Task<PaginatedResponse<RecipeDetailResponse>> ListAsync(int page, int pageSize, Guid? cuisineUuid, Guid? difficultyUuid, Guid? categoryUuid, string? search, CancellationToken ct = default);
    Task<RecipeDetailResponse> CreateAsync(Guid chefUuid, CreateRecipeRequest request, CancellationToken ct = default);
    Task<RecipeDetailResponse> UpdateAsync(Guid uuid, UpdateRecipeRequest request, CancellationToken ct = default);
    Task<RecipeDetailResponse> ToggleActiveAsync(Guid uuid, CancellationToken ct = default);
    Task DeleteAsync(Guid uuid, SoftDeleteRequest request, CancellationToken ct = default);
}
