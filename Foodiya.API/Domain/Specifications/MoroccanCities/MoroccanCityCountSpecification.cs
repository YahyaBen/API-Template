using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.MoroccanCities;

/// <summary>
/// Specification: Count Moroccan cities using the same filters as MoroccanCityListSpecification.
/// </summary>
public sealed class MoroccanCityCountSpecification : BaseSpecification<MoroccanCity>
{
    public MoroccanCityCountSpecification(
        int? regionId = null,
        bool? isActive = null,
        string? search = null)
        : base(city =>
            (!regionId.HasValue || city.RegionId == regionId.Value)
            && (!isActive.HasValue || city.IsActive == isActive.Value)
            && (string.IsNullOrWhiteSpace(search)
                || city.Name.ToLower().Contains(search.Trim().ToLower())
                || city.Slug.ToLower().Contains(search.Trim().ToLower())
                || city.Region.Name.ToLower().Contains(search.Trim().ToLower())))
    {
    }
}
