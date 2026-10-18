using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.FoodCategories;

/// <summary>
/// Specification: Paginated food category list with optional active filter and free-text search.
/// </summary>
public sealed class FoodCategoryListSpecification : BaseSpecification<FoodCategory>
{
    public FoodCategoryListSpecification(
        int page,
        int pageSize,
        bool? isActive = null,
        string? search = null)
        : base(fc =>
            (!isActive.HasValue || fc.IsActive == isActive.Value)
            && (string.IsNullOrWhiteSpace(search)
                || fc.Name.ToLower().Contains(search.Trim().ToLower())
                || (fc.Description != null && fc.Description.ToLower().Contains(search.Trim().ToLower()))
                || fc.Code.ToLower().Contains(search.Trim().ToLower())))
    {
        ApplyOrderBy(fc => fc.SortOrder);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
