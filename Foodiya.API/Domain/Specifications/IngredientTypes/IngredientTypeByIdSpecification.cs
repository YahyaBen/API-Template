using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.IngredientTypes;

/// <summary>
/// Specification: Ingredient type detail by ID.
/// </summary>
public sealed class IngredientTypeByIdSpecification : BaseSpecification<IngredientType>
{
    public IngredientTypeByIdSpecification(int id) : base(it => it.Id == id)
    {
    }
}
