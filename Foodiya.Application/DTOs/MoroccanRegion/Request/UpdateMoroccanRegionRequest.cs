using System.ComponentModel.DataAnnotations;

namespace Foodiya.Application.DTOs.MoroccanRegion.Request;

public sealed class UpdateMoroccanRegionRequest
{
    [StringLength(20)]
    public string? Code { get; set; }

    [StringLength(150)]
    public string? Name { get; set; }

    [Range(0, int.MaxValue)]
    public int? SortOrder { get; set; }

    public bool? IsActive { get; set; }
}
