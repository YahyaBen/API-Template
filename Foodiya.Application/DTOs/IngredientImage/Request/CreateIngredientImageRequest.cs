using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Foodiya.Application.DTOs.IngredientImage.Request;

public sealed class CreateIngredientImageRequest
{
    [Required]
    public Guid IngredientUuid { get; set; }

    [JsonIgnore]
    public int IngredientId { get; set; }

    [Required, StringLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    [StringLength(250)]
    public string? AltText { get; set; }

    public bool IsPrimary { get; set; }

    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }
}
