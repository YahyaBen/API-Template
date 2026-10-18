using System.ComponentModel.DataAnnotations;

namespace Foodiya.Application.DTOs.IngredientType.Request;

public sealed class CreateIngredientTypeRequest
{
    [Required, StringLength(30)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Label { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }

    [StringLength(500)]
    public string? IconUrl { get; set; }

    [StringLength(20)]
    public string? Color { get; set; }

    public bool IsActive { get; set; } = true;
}
