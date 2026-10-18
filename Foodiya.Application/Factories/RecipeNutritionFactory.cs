using Foodiya.Application.DTOs.RecipeNutrition.Request;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Factories;

public sealed class RecipeNutritionFactory : IRecipeNutritionFactory
{
    public RecipeNutrition Create(CreateRecipeNutritionRequest request) => new()
    {
        RecipeId = request.RecipeId,
        CaloriesPerServing = request.CaloriesPerServing,
        ProteinGrams = request.ProteinGrams,
        CarbsGrams = request.CarbsGrams,
        FatGrams = request.FatGrams
    };

    public void Update(RecipeNutrition recipeNutrition, UpdateRecipeNutritionRequest request)
    {
        if (request.CaloriesPerServing.HasValue)
            recipeNutrition.CaloriesPerServing = request.CaloriesPerServing.Value;

        if (request.ClearProteinGrams)
            recipeNutrition.ProteinGrams = null;
        else if (request.ProteinGrams.HasValue)
            recipeNutrition.ProteinGrams = request.ProteinGrams.Value;

        if (request.ClearCarbsGrams)
            recipeNutrition.CarbsGrams = null;
        else if (request.CarbsGrams.HasValue)
            recipeNutrition.CarbsGrams = request.CarbsGrams.Value;

        if (request.ClearFatGrams)
            recipeNutrition.FatGrams = null;
        else if (request.FatGrams.HasValue)
            recipeNutrition.FatGrams = request.FatGrams.Value;
    }
}
