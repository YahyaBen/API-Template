using Foodiya.Application.DTOs.Ingredient.Request;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Factories;

public interface IIngredientFactory
{
    Ingredient Create(CreateIngredientRequest request);
    void Update(Ingredient ingredient, UpdateIngredientRequest request);
}
