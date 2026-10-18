using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Application.DTOs.RecipeShare.Request;
using Foodiya.Application.DTOs.RecipeShare.Response;

namespace Foodiya.Application.Interfaces.Services;

public interface IRecipeShareService
{
    Task<RecipeShareDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default);
    Task<PaginatedResponse<RecipeShareDetailResponse>> ListAsync(int page, int pageSize, Guid? recipeUuid, Guid? sharedByUserUuid, Guid? sharedWithUserUuid, string? channel, string? search, CancellationToken ct = default);
    Task<RecipeShareDetailResponse> CreateAsync(CreateRecipeShareRequest request, CancellationToken ct = default);
    Task<RecipeShareDetailResponse> UpdateAsync(Guid uuid, UpdateRecipeShareRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid uuid, CancellationToken ct = default);
}
