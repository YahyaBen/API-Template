using System.ComponentModel.DataAnnotations;

namespace Foodiya.Application.DTOs.PresetAvatarImage.Request;

public sealed class UpdatePresetAvatarImageRequest
{
    [StringLength(50)]
    public string? Code { get; set; }

    [StringLength(100)]
    public string? Label { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [StringLength(20)]
    public string? BackgroundColor { get; set; }

    public bool ClearBackgroundColor { get; set; }

    [Range(0, int.MaxValue)]
    public int? SortOrder { get; set; }

    public bool? IsActive { get; set; }
}
