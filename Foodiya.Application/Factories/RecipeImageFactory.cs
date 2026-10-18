using Foodiya.Application.DTOs.RecipeImage.Request;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Domain.Exceptions;
using Foodiya.Domain.Extensions;
using Foodiya.Domain.Models;
using static Foodiya.Application.Factories.Helpers.EntityNormalizationHelper;

namespace Foodiya.Application.Factories;

public sealed class RecipeImageFactory : IRecipeImageFactory
{
    public RecipeImage Create(CreateRecipeImageRequest request) => new()
    {
        RecipeId = request.RecipeId,
        ImageUrl = Required(request.ImageUrl, nameof(request.ImageUrl)),
        AltText = Optional(request.AltText),
        IsPrimary = request.IsPrimary,
        SortOrder = request.SortOrder,
        Code = EntityCodeGenerator.For("RIM")
    };

    public void Update(RecipeImage recipeImage, UpdateRecipeImageRequest request)
    {
        if (request.ClearAltText && request.AltText is not null)
            throw new FoodiyaBadRequestException("Provide AltText or ClearAltText, not both.");

        if (request.ImageUrl is not null)
            recipeImage.ImageUrl = Required(request.ImageUrl, nameof(request.ImageUrl));

        if (request.ClearAltText)
            recipeImage.AltText = null;
        else if (request.AltText is not null)
            recipeImage.AltText = Optional(request.AltText);

        if (request.IsPrimary.HasValue)
            recipeImage.IsPrimary = request.IsPrimary.Value;

        if (request.SortOrder.HasValue)
            recipeImage.SortOrder = request.SortOrder.Value;
    }
}
