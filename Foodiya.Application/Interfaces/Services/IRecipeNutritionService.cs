using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Application.DTOs.RecipeNutrition.Request;
using Foodiya.Application.DTOs.RecipeNutrition.Response;

namespace Foodiya.Application.Interfaces.Services;

public interface IRecipeNutritionService
{
    Task<RecipeNutritionDetailResponse?> GetByIdAsync(Guid recipeUuid, CancellationToken ct = default);
    Task<PaginatedResponse<RecipeNutritionDetailResponse>> ListAsync(int page, int pageSize, Guid? recipeUuid, string? search, CancellationToken ct = default);
    Task<RecipeNutritionDetailResponse> CreateAsync(CreateRecipeNutritionRequest request, CancellationToken ct = default);
    Task<RecipeNutritionDetailResponse> UpdateAsync(Guid recipeUuid, UpdateRecipeNutritionRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid recipeUuid, CancellationToken ct = default);
}
