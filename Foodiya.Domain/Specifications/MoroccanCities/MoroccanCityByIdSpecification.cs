using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.MoroccanCities;

/// <summary>
/// Specification: Moroccan city detail by ID with linked region.
/// </summary>
public sealed class MoroccanCityByIdSpecification : BaseSpecification<MoroccanCity>
{
    public MoroccanCityByIdSpecification(int id) : base(city => city.Id == id)
    {
        AddInclude(city => city.Region);
    }
}
