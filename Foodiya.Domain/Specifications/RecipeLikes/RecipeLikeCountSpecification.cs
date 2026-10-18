using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.RecipeLikes;

/// <summary>
/// Specification: Count recipe likes using the same filters as RecipeLikeListSpecification.
/// </summary>
public sealed class RecipeLikeCountSpecification : BaseSpecification<RecipeLike>
{
    public RecipeLikeCountSpecification(
        int? recipeId = null,
        int? userId = null,
        string? search = null)
        : base(rl =>
            (!recipeId.HasValue || rl.RecipeId == recipeId.Value)
            && (!userId.HasValue || rl.UserId == userId.Value)
            && (string.IsNullOrWhiteSpace(search)
                || rl.Recipe.Title.ToLower().Contains(search.Trim().ToLower())
                || rl.User.UserName.ToLower().Contains(search.Trim().ToLower())
                || rl.User.FirstName.ToLower().Contains(search.Trim().ToLower())
                || rl.User.LastName.ToLower().Contains(search.Trim().ToLower())))
    {
    }
}
