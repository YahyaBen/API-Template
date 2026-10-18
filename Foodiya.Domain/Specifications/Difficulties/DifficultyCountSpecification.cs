using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.Difficulties;

/// <summary>
/// Specification: Count difficulties using the same filters as DifficultyListSpecification.
/// </summary>
public sealed class DifficultyCountSpecification : BaseSpecification<Difficulty>
{
    public DifficultyCountSpecification(
        bool? isActive = null,
        string? search = null)
        : base(d =>
            (!isActive.HasValue || d.IsActive == isActive.Value)
            && (string.IsNullOrWhiteSpace(search)
                || d.Name.ToLower().Contains(search.Trim().ToLower())
                || d.Code.ToLower().Contains(search.Trim().ToLower())))
    {
    }
}
