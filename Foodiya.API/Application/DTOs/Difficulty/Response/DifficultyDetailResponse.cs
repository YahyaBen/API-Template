namespace Foodiya.Application.DTOs.Difficulty.Response;

public sealed class DifficultyDetailResponse
{
    public Guid Uuid { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public int SortOrder { get; set; }
    public string? IconUrl { get; set; }
    public string? Color { get; set; }
    public DateTime DateInsert { get; set; }
    public DateTime? DateModif { get; set; }
}
