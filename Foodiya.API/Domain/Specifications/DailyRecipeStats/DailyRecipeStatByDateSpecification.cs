using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.DailyRecipeStats;

/// <summary>
/// Specification: Daily recipe stat for a specific date.
/// </summary>
public sealed class DailyRecipeStatByDateSpecification : BaseSpecification<DailyRecipeStat>
{
    public DailyRecipeStatByDateSpecification(DateTime date) : base(s => s.StatsDate == date) { }
}
