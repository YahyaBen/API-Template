using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.Cuisines;

/// <summary>
/// Specification: Cuisine detail by ID.
/// </summary>
public sealed class CuisineByIdSpecification : BaseSpecification<Cuisine>
{
    public CuisineByIdSpecification(int id) : base(c => c.Id == id)
    {
    }
}
