namespace Foodiya.Application.DTOs.Ingredient.Response;

public sealed class IngredientDetailResponse
{
    public Guid Uuid { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid IngredientTypeUuid { get; set; }
    public string IngredientTypeLabel { get; set; } = string.Empty;
    public Guid? DefaultUnitUuid { get; set; }
    public string? DefaultUnitLabel { get; set; }
    public decimal? CaloriesPer100g { get; set; }
    public bool IsActive { get; set; }
}
