using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.MoroccanCities;

/// <summary>
/// Specification: Paginated Moroccan city list with optional region filter, active filter, and free-text search.
/// </summary>
public sealed class MoroccanCityListSpecification : BaseSpecification<MoroccanCity>
{
    public MoroccanCityListSpecification(
        int page,
        int pageSize,
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
        AddInclude(city => city.Region);

        ApplyOrderBy(city => city.SortOrder);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
