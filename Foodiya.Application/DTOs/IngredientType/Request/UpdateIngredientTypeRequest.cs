using System.ComponentModel.DataAnnotations;

namespace Foodiya.Application.DTOs.IngredientType.Request;

public sealed class UpdateIngredientTypeRequest
{
    [StringLength(30)]
    public string? Code { get; set; }

    [StringLength(80)]
    public string? Label { get; set; }

    [Range(0, int.MaxValue)]
    public int? SortOrder { get; set; }

    [StringLength(500)]
    public string? IconUrl { get; set; }

    [StringLength(20)]
    public string? Color { get; set; }

    public bool? IsActive { get; set; }
}
