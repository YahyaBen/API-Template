using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.FoodCategories;

/// <summary>
/// Specification: Count food categories using the same filters as FoodCategoryListSpecification.
/// </summary>
public sealed class FoodCategoryCountSpecification : BaseSpecification<FoodCategory>
{
    public FoodCategoryCountSpecification(
        bool? isActive = null,
        string? search = null)
        : base(fc =>
            (!isActive.HasValue || fc.IsActive == isActive.Value)
            && (string.IsNullOrWhiteSpace(search)
                || fc.Name.ToLower().Contains(search.Trim().ToLower())
                || (fc.Description != null && fc.Description.ToLower().Contains(search.Trim().ToLower()))
                || fc.Code.ToLower().Contains(search.Trim().ToLower())))
    {
    }
}
