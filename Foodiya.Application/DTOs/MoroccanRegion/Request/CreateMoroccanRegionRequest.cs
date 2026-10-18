using System.ComponentModel.DataAnnotations;

namespace Foodiya.Application.DTOs.MoroccanRegion.Request;

public sealed class CreateMoroccanRegionRequest
{
    [Required, StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
