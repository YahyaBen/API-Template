using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.RecipeNutritions;

/// <summary>
/// Specification: Count recipe nutritions using the same filters as RecipeNutritionListSpecification.
/// </summary>
public sealed class RecipeNutritionCountSpecification : BaseSpecification<RecipeNutrition>
{
    public RecipeNutritionCountSpecification(
        int? recipeId = null,
        string? search = null)
        : base(rn =>
            rn.Recipe.DeletedAt == null
            && (!recipeId.HasValue || rn.RecipeId == recipeId.Value)
            && (string.IsNullOrWhiteSpace(search)
                || rn.Recipe.Title.ToLower().Contains(search.Trim().ToLower())))
    {
    }
}
