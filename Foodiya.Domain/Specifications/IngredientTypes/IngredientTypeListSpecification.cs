using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.IngredientTypes;

/// <summary>
/// Specification: Paginated ingredient type list with optional active filter and free-text search.
/// </summary>
public sealed class IngredientTypeListSpecification : BaseSpecification<IngredientType>
{
    public IngredientTypeListSpecification(
        int page,
        int pageSize,
        bool? isActive = null,
        string? search = null)
        : base(it =>
            (!isActive.HasValue || it.IsActive == isActive.Value)
            && (string.IsNullOrWhiteSpace(search)
                || it.Label.ToLower().Contains(search.Trim().ToLower())
                || it.Code.ToLower().Contains(search.Trim().ToLower())))
    {
        ApplyOrderBy(it => it.SortOrder);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
