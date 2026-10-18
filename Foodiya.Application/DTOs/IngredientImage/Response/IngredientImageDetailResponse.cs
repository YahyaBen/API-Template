namespace Foodiya.Application.DTOs.IngredientImage.Response;

public sealed class IngredientImageDetailResponse
{
    public Guid Uuid { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid IngredientUuid { get; set; }
    public string IngredientName { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}
