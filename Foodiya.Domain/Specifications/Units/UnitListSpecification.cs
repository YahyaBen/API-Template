using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.Units;

/// <summary>
/// Specification: Paginated unit list with optional active filter and free-text search.
/// </summary>
public sealed class UnitListSpecification : BaseSpecification<Unit>
{
    public UnitListSpecification(
        int page,
        int pageSize,
        bool? isActive = null,
        string? search = null)
        : base(unit =>
            (!isActive.HasValue || unit.IsActive == isActive.Value)
            && (string.IsNullOrWhiteSpace(search)
                || unit.Label.ToLower().Contains(search.Trim().ToLower())
                || unit.Code.ToLower().Contains(search.Trim().ToLower())))
    {
        ApplyOrderBy(unit => unit.SortOrder);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
