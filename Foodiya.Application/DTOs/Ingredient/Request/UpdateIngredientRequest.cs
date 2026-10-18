using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Foodiya.Application.DTOs.Ingredient.Request;

public sealed class UpdateIngredientRequest
{
    [StringLength(150)]
    public string? Name { get; set; }

    public Guid? IngredientTypeUuid { get; set; }

    public Guid? DefaultUnitUuid { get; set; }

    [JsonIgnore]
    public int? IngredientTypeId { get; set; }

    [JsonIgnore]
    public int? DefaultUnitId { get; set; }

    public bool ClearDefaultUnit { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal? CaloriesPer100g { get; set; }

    public bool ClearCaloriesPer100g { get; set; }

    public bool? IsActive { get; set; }
}
