using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.Units;

/// <summary>
/// Specification: Unit detail by ID.
/// </summary>
public sealed class UnitByIdSpecification : BaseSpecification<Unit>
{
    public UnitByIdSpecification(int id) : base(unit => unit.Id == id)
    {
    }
}
