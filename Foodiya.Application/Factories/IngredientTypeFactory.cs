using Foodiya.Application.DTOs.IngredientType.Request;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using static Foodiya.Application.Factories.Helpers.EntityNormalizationHelper;

namespace Foodiya.Application.Factories;

public sealed class IngredientTypeFactory : IIngredientTypeFactory
{
    public IngredientType Create(CreateIngredientTypeRequest request) => new()
    {
        Code = Code(request.Code, nameof(request.Code)),
        Label = Required(request.Label, nameof(request.Label)),
        SortOrder = request.SortOrder,
        IconUrl = Optional(request.IconUrl),
        Color = Optional(request.Color),
        IsActive = request.IsActive
    };

    public void Update(IngredientType ingredientType, UpdateIngredientTypeRequest request, DateTime utcNow)
    {
        if (request.Code is not null)
            ingredientType.Code = Code(request.Code, nameof(request.Code));

        if (request.Label is not null)
            ingredientType.Label = Required(request.Label, nameof(request.Label));

        if (request.SortOrder.HasValue)
            ingredientType.SortOrder = request.SortOrder.Value;

        if (request.IconUrl is not null)
            ingredientType.IconUrl = Optional(request.IconUrl);

        if (request.Color is not null)
            ingredientType.Color = Optional(request.Color);

        if (request.IsActive.HasValue)
            ingredientType.IsActive = request.IsActive.Value;

        ingredientType.DateModif = utcNow;
    }
}
