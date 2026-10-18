using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.RecipeSteps;

/// <summary>
/// Specification: Recipe step detail by ID with linked recipe.
/// </summary>
public sealed class RecipeStepByIdSpecification : BaseSpecification<RecipeStep>
{
    public RecipeStepByIdSpecification(int id)
        : base(rs => rs.Id == id && rs.Recipe.DeletedAt == null)
    {
        AddInclude(rs => rs.Recipe);
    }
}
