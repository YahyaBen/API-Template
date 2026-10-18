using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.Ingredients;

/// <summary>
/// Specification: Ingredient detail by ID with related type and default unit.
/// </summary>
public sealed class IngredientByIdSpecification : BaseSpecification<Ingredient>
{
    public IngredientByIdSpecification(int id) : base(i => i.Id == id)
    {
        AddInclude(i => i.IngredientType);
        AddInclude(i => i.DefaultUnit!);
    }
}
