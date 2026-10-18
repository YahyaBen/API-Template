using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.DailyRecipeStats;

/// <summary>
/// Specification: Daily recipe stat by ID.
/// </summary>
public sealed class DailyRecipeStatByIdSpecification : BaseSpecification<DailyRecipeStat>
{
    public DailyRecipeStatByIdSpecification(int id) : base(s => s.Id == id) { }
}
