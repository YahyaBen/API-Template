using Foodiya.Application.DTOs.Ingredient.Request;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Domain.Exceptions;
using Foodiya.Domain.Extensions;
using Foodiya.Domain.Models;
using static Foodiya.Application.Factories.Helpers.EntityNormalizationHelper;

namespace Foodiya.Application.Factories;

public sealed class IngredientFactory : IIngredientFactory
{
    public Ingredient Create(CreateIngredientRequest request) => new()
    {
        Name = Required(request.Name, nameof(request.Name)),
        IngredientTypeId = request.IngredientTypeId,
        DefaultUnitId = request.DefaultUnitId,
        CaloriesPer100g = request.CaloriesPer100g,
        IsActive = request.IsActive,
        Code = EntityCodeGenerator.For("ING")
    };

    public void Update(Ingredient ingredient, UpdateIngredientRequest request)
    {
        if (request.ClearDefaultUnit && request.DefaultUnitId.HasValue)
            throw new FoodiyaBadRequestException("Provide DefaultUnitId or ClearDefaultUnit, not both.");

        if (request.ClearCaloriesPer100g && request.CaloriesPer100g.HasValue)
            throw new FoodiyaBadRequestException("Provide CaloriesPer100g or ClearCaloriesPer100g, not both.");

        if (request.Name is not null)
            ingredient.Name = Required(request.Name, nameof(request.Name));

        if (request.IngredientTypeId.HasValue)
            ingredient.IngredientTypeId = request.IngredientTypeId.Value;

        if (request.ClearDefaultUnit)
            ingredient.DefaultUnitId = null;
        else if (request.DefaultUnitId.HasValue)
            ingredient.DefaultUnitId = request.DefaultUnitId.Value;

        if (request.ClearCaloriesPer100g)
            ingredient.CaloriesPer100g = null;
        else if (request.CaloriesPer100g.HasValue)
            ingredient.CaloriesPer100g = request.CaloriesPer100g.Value;

        if (request.IsActive.HasValue)
            ingredient.IsActive = request.IsActive.Value;
    }
}
