using Foodiya.Application.DTOs.RecipeLike.Request;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Factories;

public sealed class RecipeLikeFactory : IRecipeLikeFactory
{
    public RecipeLike Create(CreateRecipeLikeRequest request) => new()
    {
        RecipeId = request.RecipeId,
        UserId = request.UserId
    };
}
