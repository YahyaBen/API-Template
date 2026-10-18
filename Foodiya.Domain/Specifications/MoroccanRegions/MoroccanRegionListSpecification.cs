using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.MoroccanRegions;

/// <summary>
/// Specification: Paginated Moroccan region list with optional active filter and free-text search.
/// </summary>
public sealed class MoroccanRegionListSpecification : BaseSpecification<MoroccanRegion>
{
    public MoroccanRegionListSpecification(
        int page,
        int pageSize,
        bool? isActive = null,
        string? search = null)
        : base(region =>
            (!isActive.HasValue || region.IsActive == isActive.Value)
            && (string.IsNullOrWhiteSpace(search)
                || region.Name.ToLower().Contains(search.Trim().ToLower())
                || region.Code.ToLower().Contains(search.Trim().ToLower())))
    {
        ApplyOrderBy(region => region.SortOrder);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
