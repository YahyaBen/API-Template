namespace Foodiya.Application.DTOs.Recipe.Response;

public sealed class RecipeNutritionResponse
{
    public decimal CaloriesPerServing { get; set; }
    public decimal? ProteinGrams { get; set; }
    public decimal? CarbsGrams { get; set; }
    public decimal? FatGrams { get; set; }
}
