namespace Foodiya.Application.DTOs.Recipe.Response;

public sealed class RecipeIngredientResponse
{
    public string IngredientName { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public string? UnitLabel { get; set; }
    public bool IsOptional { get; set; }
    public string? Notes { get; set; }
    public int SortOrder { get; set; }
}
