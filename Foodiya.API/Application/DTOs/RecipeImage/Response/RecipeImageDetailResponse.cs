namespace Foodiya.Application.DTOs.RecipeImage.Response;

public sealed class RecipeImageDetailResponse
{
    public Guid Uuid { get; set; }
    public string Code { get; set; } = string.Empty;
    public Guid RecipeUuid { get; set; }
    public string RecipeTitle { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? AltText { get; set; }
    public bool IsPrimary { get; set; }
    public int SortOrder { get; set; }
}
