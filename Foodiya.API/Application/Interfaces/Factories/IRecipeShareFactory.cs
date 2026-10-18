using Foodiya.Application.DTOs.RecipeShare.Request;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Factories;

public interface IRecipeShareFactory
{
    RecipeShare Create(CreateRecipeShareRequest request);
    void Update(RecipeShare recipeShare, UpdateRecipeShareRequest request);
}
