using Foodiya.Domain.Models;
using Foodiya.Domain.Specifications;

namespace Foodiya.Application.Interfaces.Services;

public interface IResourceAuthorization
{
    Task<int> GetActorIdAsync(Guid suppliedUserUuid, CancellationToken ct);
    Task RequireChefOwnerAsync(int chefId, CancellationToken ct);
    Task RequireRecipeOwnerAsync(int recipeId, CancellationToken ct);
    Task RequireShareOwnerAsync(RecipeShare share, CancellationToken ct);
    Task<RecipeReadAccess> GetReadAccessAsync(CancellationToken ct);
    Task RequireRecipeReadAsync(int recipeId, CancellationToken ct);
    Task RequireRecipeShareAsync(int recipeId, CancellationToken ct);
    Task RequireShareReadAsync(RecipeShare share, CancellationToken ct);
}
