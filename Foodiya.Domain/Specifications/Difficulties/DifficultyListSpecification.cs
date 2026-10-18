using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.Difficulties;

/// <summary>
/// Specification: Paginated difficulty list with optional active filter and free-text search.
/// </summary>
public sealed class DifficultyListSpecification : BaseSpecification<Difficulty>
{
    public DifficultyListSpecification(
        int page,
        int pageSize,
        bool? isActive = null,
        string? search = null)
        : base(d =>
            (!isActive.HasValue || d.IsActive == isActive.Value)
            && (string.IsNullOrWhiteSpace(search)
                || d.Name.ToLower().Contains(search.Trim().ToLower())
                || d.Code.ToLower().Contains(search.Trim().ToLower())))
    {
        ApplyOrderBy(d => d.SortOrder);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
