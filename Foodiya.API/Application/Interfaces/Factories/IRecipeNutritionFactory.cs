using Foodiya.Application.DTOs.RecipeNutrition.Request;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Factories;

public interface IRecipeNutritionFactory
{
    RecipeNutrition Create(CreateRecipeNutritionRequest request);
    void Update(RecipeNutrition recipeNutrition, UpdateRecipeNutritionRequest request);
}
