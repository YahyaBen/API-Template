using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.IngredientTypes;

/// <summary>
/// Specification: Count ingredient types using the same filters as IngredientTypeListSpecification.
/// </summary>
public sealed class IngredientTypeCountSpecification : BaseSpecification<IngredientType>
{
    public IngredientTypeCountSpecification(
        bool? isActive = null,
        string? search = null)
        : base(it =>
            (!isActive.HasValue || it.IsActive == isActive.Value)
            && (string.IsNullOrWhiteSpace(search)
                || it.Label.ToLower().Contains(search.Trim().ToLower())
                || it.Code.ToLower().Contains(search.Trim().ToLower())))
    {
    }
}
