using Foodiya.Application.DTOs.RecipeShare.Request;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Domain.Extensions;
using Foodiya.Domain.Models;
using static Foodiya.Application.Factories.Helpers.EntityNormalizationHelper;

namespace Foodiya.Application.Factories;

public sealed class RecipeShareFactory : IRecipeShareFactory
{
    public RecipeShare Create(CreateRecipeShareRequest request) => new()
    {
        RecipeId = request.RecipeId,
        SharedByUserId = request.SharedByUserId,
        SharedWithUserId = request.SharedWithUserId,
        ShareChannel = Required(request.ShareChannel, nameof(request.ShareChannel)),
        ShareMessage = Optional(request.ShareMessage),
        Code = EntityCodeGenerator.For("RSH")
    };

    public void Update(RecipeShare recipeShare, UpdateRecipeShareRequest request)
    {
        if (request.ClearSharedWithUser)
            recipeShare.SharedWithUserId = null;
        else if (request.SharedWithUserId.HasValue)
            recipeShare.SharedWithUserId = request.SharedWithUserId.Value;

        if (request.ShareChannel is not null)
            recipeShare.ShareChannel = Required(request.ShareChannel, nameof(request.ShareChannel));

        if (request.ClearShareMessage)
            recipeShare.ShareMessage = null;
        else if (request.ShareMessage is not null)
            recipeShare.ShareMessage = Optional(request.ShareMessage);
    }
}
