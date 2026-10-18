using System.ComponentModel.DataAnnotations;

namespace Foodiya.Application.DTOs.Unit.Request;

public sealed class UpdateUnitRequest
{
    [StringLength(20)]
    public string? Code { get; set; }

    [StringLength(50)]
    public string? Label { get; set; }

    [Range(0, int.MaxValue)]
    public int? SortOrder { get; set; }

    public bool? IsActive { get; set; }
}
