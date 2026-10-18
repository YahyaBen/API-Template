using System.ComponentModel.DataAnnotations;

namespace Foodiya.Application.DTOs.Difficulty.Request;

public sealed class UpdateDifficultyRequest
{
    [StringLength(50)]
    public string? Name { get; set; }

    [StringLength(30)]
    public string? Code { get; set; }

    [Range(0, int.MaxValue)]
    public int? SortOrder { get; set; }

    [StringLength(500)]
    public string? IconUrl { get; set; }

    [StringLength(20)]
    public string? Color { get; set; }

    public bool? IsActive { get; set; }
}
