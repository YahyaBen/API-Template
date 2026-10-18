using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.RecipeNutritions;

/// <summary>
/// Specification: Recipe nutrition detail by recipe ID with linked recipe.
/// </summary>
public sealed class RecipeNutritionByIdSpecification : BaseSpecification<RecipeNutrition>
{
    public RecipeNutritionByIdSpecification(int recipeId)
        : base(rn => rn.RecipeId == recipeId && rn.Recipe.DeletedAt == null)
    {
        AddInclude(rn => rn.Recipe);
    }
}
