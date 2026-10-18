using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.Ingredients;

/// <summary>
/// Specification: Paginated ingredient list with optional type filter, active filter, and free-text search.
/// </summary>
public sealed class IngredientListSpecification : BaseSpecification<Ingredient>
{
    public IngredientListSpecification(
        int page,
        int pageSize,
        int? ingredientTypeId = null,
        bool? isActive = null,
        string? search = null)
        : base(i =>
            (!ingredientTypeId.HasValue || i.IngredientTypeId == ingredientTypeId.Value)
            && (!isActive.HasValue || i.IsActive == isActive.Value)
            && (string.IsNullOrWhiteSpace(search)
                || i.Name.ToLower().Contains(search.Trim().ToLower())))
    {
        AddInclude(i => i.IngredientType);
        AddInclude(i => i.DefaultUnit!);

        ApplyOrderBy(i => i.Name);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
