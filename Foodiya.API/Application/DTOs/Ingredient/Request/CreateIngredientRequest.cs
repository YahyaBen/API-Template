using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Foodiya.Application.DTOs.Ingredient.Request;

public sealed class CreateIngredientRequest
{
    [Required, StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    public Guid IngredientTypeUuid { get; set; }

    [JsonIgnore]
    public int IngredientTypeId { get; set; }

    public Guid? DefaultUnitUuid { get; set; }

    [JsonIgnore]
    public int? DefaultUnitId { get; set; }

    [Range(typeof(decimal), "0", "99999999.99", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public decimal? CaloriesPer100g { get; set; }

    public bool IsActive { get; set; } = true;
}
