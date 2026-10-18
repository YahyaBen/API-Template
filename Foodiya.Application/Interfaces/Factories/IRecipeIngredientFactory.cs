using Foodiya.Application.DTOs.RecipeIngredient.Request;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Factories;

public interface IRecipeIngredientFactory
{
    RecipeIngredient Create(CreateRecipeIngredientRequest request);
    void Update(RecipeIngredient recipeIngredient, UpdateRecipeIngredientRequest request);
}
