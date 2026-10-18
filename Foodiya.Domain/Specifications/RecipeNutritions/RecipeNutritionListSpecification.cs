using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.RecipeNutritions;

/// <summary>
/// Specification: Paginated recipe nutrition list with optional recipe filter and free-text search.
/// </summary>
public sealed class RecipeNutritionListSpecification : BaseSpecification<RecipeNutrition>
{
    public RecipeNutritionListSpecification(
        int page,
        int pageSize,
        int? recipeId = null,
        string? search = null)
        : base(rn =>
            rn.Recipe.DeletedAt == null
            && (!recipeId.HasValue || rn.RecipeId == recipeId.Value)
            && (string.IsNullOrWhiteSpace(search)
                || rn.Recipe.Title.ToLower().Contains(search.Trim().ToLower())))
    {
        AddInclude(rn => rn.Recipe);

        ApplyOrderBy(rn => rn.Recipe.Title);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
