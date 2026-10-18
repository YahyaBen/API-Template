using Foodiya.Application.DTOs.IngredientType.Request;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Factories;

public interface IIngredientTypeFactory
{
    IngredientType Create(CreateIngredientTypeRequest request);
    void Update(IngredientType ingredientType, UpdateIngredientTypeRequest request, DateTime utcNow);
}
