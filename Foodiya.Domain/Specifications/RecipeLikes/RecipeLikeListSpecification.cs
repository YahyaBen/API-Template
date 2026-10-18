using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.RecipeLikes;

/// <summary>
/// Specification: Paginated recipe like list with optional recipe filter, user filter, and free-text search.
/// </summary>
public sealed class RecipeLikeListSpecification : BaseSpecification<RecipeLike>
{
    public RecipeLikeListSpecification(
        int page,
        int pageSize,
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
        AddInclude(rl => rl.Recipe);
        AddInclude(rl => rl.User);

        ApplyOrderByDescending(rl => rl.LikedAt);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
