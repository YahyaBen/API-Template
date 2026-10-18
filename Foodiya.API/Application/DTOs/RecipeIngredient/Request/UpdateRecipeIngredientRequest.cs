using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Foodiya.Application.DTOs.RecipeIngredient.Request;

public sealed class UpdateRecipeIngredientRequest
{
    [Range(0.01, 99999)]
    public decimal? Quantity { get; set; }

    public Guid? UnitUuid { get; set; }

    [JsonIgnore]
    public int? UnitId { get; set; }

    public bool ClearUnitId { get; set; }

    public bool? IsOptional { get; set; }

    [StringLength(250)]
    public string? Notes { get; set; }

    public bool ClearNotes { get; set; }

    [Range(0, 100)]
    public int? SortOrder { get; set; }
}
