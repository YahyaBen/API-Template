using Foodiya.Application.DTOs.RecipeStep.Request;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Factories;

public interface IRecipeStepFactory
{
    RecipeStep Create(CreateRecipeStepItemRequest request);
    void Update(RecipeStep recipeStep, UpdateRecipeStepItemRequest request);
}
