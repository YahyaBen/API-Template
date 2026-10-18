using System.ComponentModel.DataAnnotations;

namespace Foodiya.Application.DTOs.RecipeImage.Request;

public sealed class UpdateRecipeImageRequest
{
    [StringLength(500)]
    public string? ImageUrl { get; set; }

    [StringLength(250)]
    public string? AltText { get; set; }

    public bool ClearAltText { get; set; }

    public bool? IsPrimary { get; set; }

    [Range(0, int.MaxValue)]
    public int? SortOrder { get; set; }
}
