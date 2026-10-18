using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.DailyRecipeStats;

/// <summary>
/// Specification: Paginated daily recipe stats list, ordered by StatsDate descending.
/// </summary>
public sealed class DailyRecipeStatListSpecification : BaseSpecification<DailyRecipeStat>
{
    public DailyRecipeStatListSpecification(int page, int pageSize)
    {
        ApplyOrderByDescending(s => s.StatsDate);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
