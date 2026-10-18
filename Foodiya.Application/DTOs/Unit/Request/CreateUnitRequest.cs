using System.ComponentModel.DataAnnotations;

namespace Foodiya.Application.DTOs.Unit.Request;

public sealed class CreateUnitRequest
{
    [Required, StringLength(20)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(50)]
    public string Label { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
