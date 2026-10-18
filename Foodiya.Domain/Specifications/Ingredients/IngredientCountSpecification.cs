using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.Ingredients;

/// <summary>
/// Specification: Count ingredients using the same filters as IngredientListSpecification.
/// </summary>
public sealed class IngredientCountSpecification : BaseSpecification<Ingredient>
{
    public IngredientCountSpecification(
        int? ingredientTypeId = null,
        bool? isActive = null,
        string? search = null)
        : base(i =>
            (!ingredientTypeId.HasValue || i.IngredientTypeId == ingredientTypeId.Value)
            && (!isActive.HasValue || i.IsActive == isActive.Value)
            && (string.IsNullOrWhiteSpace(search)
                || i.Name.ToLower().Contains(search.Trim().ToLower())))
    {
    }
}
