using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.DailyRecipeStats;

/// <summary>
/// Specification: Most recent daily recipe stat (last entry by StatsDate).
/// </summary>
public sealed class DailyRecipeStatLastSpecification : BaseSpecification<DailyRecipeStat>
{
    public DailyRecipeStatLastSpecification()
    {
        ApplyOrderByDescending(s => s.StatsDate);
        ApplyPaging(0, 1);
    }
}
