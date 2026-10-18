using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.IngredientImages;

/// <summary>
/// Specification: Ingredient image detail by ID with linked ingredient.
/// </summary>
public sealed class IngredientImageByIdSpecification : BaseSpecification<IngredientImage>
{
    public IngredientImageByIdSpecification(int id) : base(image => image.Id == id)
    {
        AddInclude(image => image.Ingredient);
    }
}
