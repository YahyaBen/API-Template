using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.Cuisines;

/// <summary>
/// Specification: Paginated cuisine list with optional active filter and free-text search.
/// </summary>
public sealed class CuisineListSpecification : BaseSpecification<Cuisine>
{
    public CuisineListSpecification(
        int page,
        int pageSize,
        bool? isActive = null,
        string? search = null)
        : base(c =>
            (!isActive.HasValue || c.IsActive == isActive.Value)
            && (string.IsNullOrWhiteSpace(search)
                || c.Name.ToLower().Contains(search.Trim().ToLower())
                || c.Code.ToLower().Contains(search.Trim().ToLower())))
    {
        ApplyOrderBy(c => c.SortOrder);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
