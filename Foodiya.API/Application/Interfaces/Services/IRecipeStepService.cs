using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Application.DTOs.RecipeStep.Request;
using Foodiya.Application.DTOs.RecipeStep.Response;

namespace Foodiya.Application.Interfaces.Services;

public interface IRecipeStepService
{
    Task<RecipeStepDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default);
    Task<PaginatedResponse<RecipeStepDetailResponse>> ListAsync(int page, int pageSize, Guid? recipeUuid, string? search, CancellationToken ct = default);
    Task<RecipeStepDetailResponse> CreateAsync(CreateRecipeStepItemRequest request, CancellationToken ct = default);
    Task<RecipeStepDetailResponse> UpdateAsync(Guid uuid, UpdateRecipeStepItemRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid uuid, CancellationToken ct = default);
}
