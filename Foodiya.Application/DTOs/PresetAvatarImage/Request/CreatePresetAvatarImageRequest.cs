using System.ComponentModel.DataAnnotations;

namespace Foodiya.Application.DTOs.PresetAvatarImage.Request;

public sealed class CreatePresetAvatarImageRequest
{
    [Required, StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(100)]
    public string Label { get; set; } = string.Empty;

    [Required, StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [StringLength(20)]
    public string? BackgroundColor { get; set; }

    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
