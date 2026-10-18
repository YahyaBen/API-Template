using Foodiya.Application.DTOs.IngredientType.Request;
using Foodiya.Application.DTOs.IngredientType.Response;
using Foodiya.Application.DTOs.Recipe.Response;

namespace Foodiya.Application.Interfaces.Services;

public interface IIngredientTypeService
{
    Task<IngredientTypeDetailResponse?> GetByIdAsync(Guid uuid, CancellationToken ct = default);
    Task<PaginatedResponse<IngredientTypeDetailResponse>> ListAsync(int page, int pageSize, bool? isActive, string? search, CancellationToken ct = default);
    Task<IngredientTypeDetailResponse> CreateAsync(CreateIngredientTypeRequest request, CancellationToken ct = default);
    Task<IngredientTypeDetailResponse> UpdateAsync(Guid uuid, UpdateIngredientTypeRequest request, CancellationToken ct = default);
    Task<IngredientTypeDetailResponse> ToggleActiveAsync(Guid uuid, CancellationToken ct = default);
    Task DeleteAsync(Guid uuid, CancellationToken ct = default);
}
