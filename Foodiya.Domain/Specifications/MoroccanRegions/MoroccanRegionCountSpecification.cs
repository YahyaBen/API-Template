using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.MoroccanRegions;

/// <summary>
/// Specification: Count Moroccan regions using the same filters as MoroccanRegionListSpecification.
/// </summary>
public sealed class MoroccanRegionCountSpecification : BaseSpecification<MoroccanRegion>
{
    public MoroccanRegionCountSpecification(
        bool? isActive = null,
        string? search = null)
        : base(region =>
            (!isActive.HasValue || region.IsActive == isActive.Value)
            && (string.IsNullOrWhiteSpace(search)
                || region.Name.ToLower().Contains(search.Trim().ToLower())
                || region.Code.ToLower().Contains(search.Trim().ToLower())))
    {
    }
}
