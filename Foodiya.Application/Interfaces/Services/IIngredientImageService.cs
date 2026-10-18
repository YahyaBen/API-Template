using Foodiya.Application.DTOs.IngredientImage.Request;
using Foodiya.Application.DTOs.IngredientImage.Response;
using Foodiya.Application.DTOs.Recipe.Response;

namespace Foodiya.Application.Interfaces.Services;

public interface IIngredientImageService
{
    Task<IngredientImageDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default);
    Task<PaginatedResponse<IngredientImageDetailResponse>> ListAsync(int page, int pageSize, Guid? ingredientUuid, bool? isPrimary, string? search, CancellationToken ct = default);
    Task<IngredientImageDetailResponse> CreateAsync(CreateIngredientImageRequest request, CancellationToken ct = default);
    Task<IngredientImageDetailResponse> UpdateAsync(Guid uuid, UpdateIngredientImageRequest request, CancellationToken ct = default);
    Task DeleteAsync(Guid uuid, CancellationToken ct = default);
}
