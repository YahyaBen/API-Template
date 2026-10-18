using Foodiya.Application.DTOs.Ingredient.Request;
using Foodiya.Application.DTOs.Ingredient.Response;
using Foodiya.Application.DTOs.Recipe.Response;

namespace Foodiya.Application.Interfaces.Services;

public interface IIngredientService
{
    Task<IngredientDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default);
    Task<PaginatedResponse<IngredientDetailResponse>> ListAsync(int page, int pageSize, Guid? ingredientTypeUuid, bool? isActive, string? search, CancellationToken ct = default);
    Task<IngredientDetailResponse> CreateAsync(CreateIngredientRequest request, CancellationToken ct = default);
    Task<IngredientDetailResponse> UpdateAsync(Guid uuid, UpdateIngredientRequest request, CancellationToken ct = default);
    Task<IngredientDetailResponse> ToggleActiveAsync(Guid uuid, CancellationToken ct = default);
    Task DeleteAsync(Guid uuid, CancellationToken ct = default);
}
