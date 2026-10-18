using Foodiya.Application.DTOs.RecipeLike.Request;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Factories;

public interface IRecipeLikeFactory
{
    RecipeLike Create(CreateRecipeLikeRequest request);
}
