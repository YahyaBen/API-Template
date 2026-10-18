using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.DailyRecipeStats;

/// <summary>
/// Specification: Count all daily recipe stats.
/// </summary>
public sealed class DailyRecipeStatCountSpecification : BaseSpecification<DailyRecipeStat>
{
    public DailyRecipeStatCountSpecification() { }
}
