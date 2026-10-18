using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Foodiya.Application.DTOs.RecipeImage.Request;

public sealed class CreateRecipeImageRequest
{
    [Required]
    public Guid RecipeUuid { get; set; }

    [JsonIgnore]
    public int RecipeId { get; set; }

    [Required, StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [StringLength(250)]
    public string? AltText { get; set; }

    public bool IsPrimary { get; set; }

    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }
}
