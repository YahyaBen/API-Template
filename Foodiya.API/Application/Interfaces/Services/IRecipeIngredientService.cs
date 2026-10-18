using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Application.DTOs.RecipeIngredient.Request;
using Foodiya.Application.DTOs.RecipeIngredient.Response;

namespace Foodiya.Application.Interfaces.Services;

public interface IRecipeIngredientService
{
    Task<RecipeIngredientDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default);
    Task<PaginatedResponse<RecipeIngredientDetailResponse>> ListAsync(int page, int pageSize, Guid? recipeUuid, Guid? ingredientUuid, Guid? unitUuid, string? search, CancellationToken ct = default);
    Task<RecipeIngredientDetailResponse> CreateAsync(CreateRecipeIngredientRequest request, CancellationToken ct = default);
    Task<RecipeIngredientDetailResponse> UpdateAsync(Guid uuid, UpdateRecipeIngredientRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid uuid, CancellationToken ct = default);
}
