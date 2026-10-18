using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Application.DTOs.RecipeImage.Request;
using Foodiya.Application.DTOs.RecipeImage.Response;

namespace Foodiya.Application.Interfaces.Services;

public interface IRecipeImageService
{
    Task<RecipeImageDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default);
    Task<PaginatedResponse<RecipeImageDetailResponse>> ListAsync(int page, int pageSize, Guid? recipeUuid, bool? isPrimary, string? search, CancellationToken ct = default);
    Task<RecipeImageDetailResponse> CreateAsync(CreateRecipeImageRequest request, CancellationToken ct = default);
    Task<RecipeImageDetailResponse> UpdateAsync(Guid uuid, UpdateRecipeImageRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid uuid, CancellationToken ct = default);
}
