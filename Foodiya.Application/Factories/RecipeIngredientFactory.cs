using Foodiya.Application.DTOs.RecipeIngredient.Request;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Domain.Exceptions;
using Foodiya.Domain.Models;
using static Foodiya.Application.Factories.Helpers.EntityNormalizationHelper;

namespace Foodiya.Application.Factories;

public sealed class RecipeIngredientFactory : IRecipeIngredientFactory
{
    public RecipeIngredient Create(CreateRecipeIngredientRequest request) => new()
    {
        RecipeId = request.RecipeId,
        IngredientId = request.IngredientId,
        Quantity = request.Quantity,
        UnitId = request.UnitId,
        IsOptional = request.IsOptional,
        Notes = Optional(request.Notes),
        SortOrder = request.SortOrder
    };

    public void Update(RecipeIngredient recipeIngredient, UpdateRecipeIngredientRequest request)
    {
        if (request.ClearUnitId && request.UnitId.HasValue)
            throw new FoodiyaBadRequestException("Provide UnitId or ClearUnitId, not both.");

        if (request.ClearNotes && request.Notes is not null)
            throw new FoodiyaBadRequestException("Provide Notes or ClearNotes, not both.");

        if (request.Quantity.HasValue)
            recipeIngredient.Quantity = request.Quantity.Value;

        if (request.ClearUnitId)
            recipeIngredient.UnitId = null;
        else if (request.UnitId.HasValue)
            recipeIngredient.UnitId = request.UnitId.Value;

        if (request.IsOptional.HasValue)
            recipeIngredient.IsOptional = request.IsOptional.Value;

        if (request.ClearNotes)
            recipeIngredient.Notes = null;
        else if (request.Notes is not null)
            recipeIngredient.Notes = Optional(request.Notes);

        if (request.SortOrder.HasValue)
            recipeIngredient.SortOrder = request.SortOrder.Value;
    }
}
