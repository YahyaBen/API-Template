using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.FoodCategories;

/// <summary>
/// Specification: Food category detail by ID.
/// </summary>
public sealed class FoodCategoryByIdSpecification : BaseSpecification<FoodCategory>
{
    public FoodCategoryByIdSpecification(int id) : base(fc => fc.Id == id)
    {
    }
}
