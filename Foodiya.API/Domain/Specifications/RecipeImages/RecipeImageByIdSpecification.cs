using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.RecipeImages;

/// <summary>
/// Specification: Recipe image detail by ID with linked recipe.
/// </summary>
public sealed class RecipeImageByIdSpecification : BaseSpecification<RecipeImage>
{
    public RecipeImageByIdSpecification(int id)
        : base(image => image.Id == id && image.Recipe.DeletedAt == null)
    {
        AddInclude(image => image.Recipe);
    }
}
