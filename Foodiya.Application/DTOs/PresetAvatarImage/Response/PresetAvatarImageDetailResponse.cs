namespace Foodiya.Application.DTOs.PresetAvatarImage.Response;

public sealed class PresetAvatarImageDetailResponse
{
    public Guid Uuid { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public string? BackgroundColor { get; set; }
    public int SortOrder { get; set; }
    public bool IsActive { get; set; }
}
