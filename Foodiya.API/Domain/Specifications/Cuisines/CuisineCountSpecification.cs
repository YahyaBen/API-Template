using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.Cuisines;

/// <summary>
/// Specification: Count cuisines using the same filters as CuisineListSpecification.
/// </summary>
public sealed class CuisineCountSpecification : BaseSpecification<Cuisine>
{
    public CuisineCountSpecification(
        bool? isActive = null,
        string? search = null)
        : base(c =>
            (!isActive.HasValue || c.IsActive == isActive.Value)
            && (string.IsNullOrWhiteSpace(search)
                || c.Name.ToLower().Contains(search.Trim().ToLower())
                || c.Code.ToLower().Contains(search.Trim().ToLower())))
    {
    }
}
