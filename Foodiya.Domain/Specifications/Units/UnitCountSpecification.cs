using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.Units;

/// <summary>
/// Specification: Count units using the same filters as UnitListSpecification.
/// </summary>
public sealed class UnitCountSpecification : BaseSpecification<Unit>
{
    public UnitCountSpecification(
        bool? isActive = null,
        string? search = null)
        : base(unit =>
            (!isActive.HasValue || unit.IsActive == isActive.Value)
            && (string.IsNullOrWhiteSpace(search)
                || unit.Label.ToLower().Contains(search.Trim().ToLower())
                || unit.Code.ToLower().Contains(search.Trim().ToLower())))
    {
    }
}
