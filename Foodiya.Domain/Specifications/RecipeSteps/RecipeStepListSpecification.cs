using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.RecipeSteps;

/// <summary>
/// Specification: Paginated recipe step list with optional recipe filter and free-text search.
/// </summary>
public sealed class RecipeStepListSpecification : BaseSpecification<RecipeStep>
{
    public RecipeStepListSpecification(
        int page,
        int pageSize,
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
        AddInclude(rs => rs.Recipe);

        ApplyOrderBy(rs => rs.StepNumber);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
