using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Foodiya.Application.DTOs.RecipeNutrition.Request;

public sealed class CreateRecipeNutritionRequest
{
    [Required]
    public Guid RecipeUuid { get; set; }

    [JsonIgnore]
    public int RecipeId { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal CaloriesPerServing { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal? ProteinGrams { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal? CarbsGrams { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal? FatGrams { get; set; }
}
