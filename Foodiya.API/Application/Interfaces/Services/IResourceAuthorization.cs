using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Services;

public interface IResourceAuthorization
{
    Task<int> GetActorIdAsync(Guid suppliedUserUuid, CancellationToken ct);
    Task RequireChefOwnerAsync(int chefId, CancellationToken ct);
    Task RequireRecipeOwnerAsync(int recipeId, CancellationToken ct);
    Task RequireShareOwnerAsync(RecipeShare share, CancellationToken ct);
}
