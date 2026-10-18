using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.MoroccanRegions;

/// <summary>
/// Specification: Moroccan region detail by ID.
/// </summary>
public sealed class MoroccanRegionByIdSpecification : BaseSpecification<MoroccanRegion>
{
    public MoroccanRegionByIdSpecification(int id) : base(region => region.Id == id)
    {
    }
}
