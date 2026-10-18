using System.Linq.Expressions;
using Foodiya.Domain.Constants;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications;

/// <summary>One SQL-translatable visibility rule for recipes and their child resources.</summary>
public sealed record RecipeReadAccess(int ActorId, bool IsAdministrator)
{
    public Expression<Func<Recipe, bool>> Recipes(bool allowUnlisted = false) => recipe =>
        recipe.DeletedAt == null && (IsAdministrator || recipe.Chef.UserId == ActorId
            || recipe.IsActive && recipe.Status == RecipeStatusConstants.Published
                && (recipe.Visibility == RecipeVisibilityConstants.Public
                    || allowUnlisted && recipe.Visibility == RecipeVisibilityConstants.Unlisted
                    || recipe.Visibility == RecipeVisibilityConstants.Private
                        && recipe.RecipeShares.Any(share => share.SharedWithUserId == ActorId
                            && (share.SharedByUserId == recipe.Chef.UserId
                                || share.SharedByUser.Role == AppRoleConstants.Admin
                                || share.SharedByUser.Role == AppRoleConstants.SuperAdmin))));

    public Expression<Func<T, bool>> Children<T>(Expression<Func<T, Recipe>> navigation, bool allowUnlisted = false)
    {
        var predicate = Recipes(allowUnlisted);
        var body = new ReplaceParameter(predicate.Parameters[0], navigation.Body).Visit(predicate.Body)!;
        return Expression.Lambda<Func<T, bool>>(body, navigation.Parameters);
    }

    private sealed class ReplaceParameter(ParameterExpression parameter, Expression replacement) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) =>
            node == parameter ? replacement : base.VisitParameter(node);
    }
}
