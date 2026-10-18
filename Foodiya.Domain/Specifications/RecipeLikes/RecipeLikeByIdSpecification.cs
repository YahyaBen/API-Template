using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.RecipeLikes;

/// <summary>
/// Specification: Recipe like detail by composite key with linked recipe and user.
/// </summary>
public sealed class RecipeLikeByIdSpecification : BaseSpecification<RecipeLike>
{
    public RecipeLikeByIdSpecification(int recipeId, int userId)
        : base(rl => rl.RecipeId == recipeId && rl.UserId == userId)
    {
        AddInclude(rl => rl.Recipe);
        AddInclude(rl => rl.User);
    }
}
