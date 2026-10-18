using System.ComponentModel.DataAnnotations;

namespace Foodiya.Application.DTOs.RecipeNutrition.Request;

public sealed class UpdateRecipeNutritionRequest
{
    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal? CaloriesPerServing { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal? ProteinGrams { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal? CarbsGrams { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal? FatGrams { get; set; }

    public bool ClearProteinGrams { get; set; }
    public bool ClearCarbsGrams { get; set; }
    public bool ClearFatGrams { get; set; }
}
