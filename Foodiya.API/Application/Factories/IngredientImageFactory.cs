using Foodiya.Application.DTOs.IngredientImage.Request;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Domain.Exceptions;
using Foodiya.Domain.Extensions;
using Foodiya.Domain.Models;
using static Foodiya.Application.Factories.Helpers.EntityNormalizationHelper;

namespace Foodiya.Application.Factories;

public sealed class IngredientImageFactory : IIngredientImageFactory
{
    public IngredientImage Create(CreateIngredientImageRequest request) => new()
    {
        IngredientId = request.IngredientId,
        ImageUrl = Required(request.ImageUrl, nameof(request.ImageUrl)),
        AltText = Optional(request.AltText),
        IsPrimary = request.IsPrimary,
        SortOrder = request.SortOrder,
        Code = EntityCodeGenerator.For("IGI")
    };

    public void Update(IngredientImage ingredientImage, UpdateIngredientImageRequest request)
    {
        if (request.ClearAltText && request.AltText is not null)
            throw new FoodiyaBadRequestException("Provide AltText or ClearAltText, not both.");

        if (request.ImageUrl is not null)
            ingredientImage.ImageUrl = Required(request.ImageUrl, nameof(request.ImageUrl));

        if (request.ClearAltText)
            ingredientImage.AltText = null;
        else if (request.AltText is not null)
            ingredientImage.AltText = Optional(request.AltText);

        if (request.IsPrimary.HasValue)
            ingredientImage.IsPrimary = request.IsPrimary.Value;

        if (request.SortOrder.HasValue)
            ingredientImage.SortOrder = request.SortOrder.Value;
    }
}
