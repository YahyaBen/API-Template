using Foodiya.Application.DTOs.RecipeImage.Request;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Factories;

public interface IRecipeImageFactory
{
    RecipeImage Create(CreateRecipeImageRequest request);
    void Update(RecipeImage recipeImage, UpdateRecipeImageRequest request);
}
