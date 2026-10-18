namespace Foodiya.Application.DTOs.IngredientType.Response;

public sealed class IngredientTypeDetailResponse
{
    public Guid Uuid { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public string? IconUrl { get; set; }
    public string? Color { get; set; }
    public DateTime DateInsert { get; set; }
    public DateTime? DateModif { get; set; }
}
