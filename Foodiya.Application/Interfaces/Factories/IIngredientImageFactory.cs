using Foodiya.Application.DTOs.IngredientImage.Request;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Factories;

public interface IIngredientImageFactory
{
    IngredientImage Create(CreateIngredientImageRequest request);
    void Update(IngredientImage ingredientImage, UpdateIngredientImageRequest request);
}
