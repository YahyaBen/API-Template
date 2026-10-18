namespace Foodiya.Application.DTOs.RecipeIngredient.Response;

public sealed class RecipeIngredientDetailResponse
{
    public Guid Uuid { get; set; }
    public Guid RecipeUuid { get; set; }
    public string Code { get; set; } = string.Empty;
    public string RecipeTitle { get; set; } = string.Empty;
    public Guid IngredientUuid { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public Guid? UnitUuid { get; set; }
    public string? UnitLabel { get; set; }
    public decimal Quantity { get; set; }
    public bool IsOptional { get; set; }
    public string? Notes { get; set; }
    public int SortOrder { get; set; }
}
