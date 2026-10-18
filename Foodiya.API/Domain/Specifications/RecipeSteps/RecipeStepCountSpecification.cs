using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.RecipeSteps;

/// <summary>
/// Specification: Count recipe steps using the same filters as RecipeStepListSpecification.
/// </summary>
public sealed class RecipeStepCountSpecification : BaseSpecification<RecipeStep>
{
    public RecipeStepCountSpecification(
        int? recipeId = null,
        string? search = null)
        : base(rs =>
            rs.Recipe.DeletedAt == null
            && (!recipeId.HasValue || rs.RecipeId == recipeId.Value)
            && (string.IsNullOrWhiteSpace(search)
                || rs.Recipe.Title.ToLower().Contains(search.Trim().ToLower())
                || (rs.Title != null && rs.Title.ToLower().Contains(search.Trim().ToLower()))
                || rs.Instruction.ToLower().Contains(search.Trim().ToLower())))
    {
    }
}
