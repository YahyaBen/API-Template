namespace Foodiya.Application.DTOs.DailyRecipeStat.Response;

public sealed class DailyRecipeStatDetailResponse
{
    public Guid Uuid { get; set; }
    public string Code { get; set; } = string.Empty;
    public DateTime StatsDate { get; set; }
    public DateTime GeneratedAtUtc { get; set; }
    public int TotalRecipes { get; set; }
    public int TotalPublishedRecipes { get; set; }
    public int NewRecipesToday { get; set; }
    public int TotalLikesToday { get; set; }
    public string? TopLikedRecipesJson { get; set; }
    public string? RecentRecipesJson { get; set; }
    public string? CuisineBreakdownJson { get; set; }
    public string? DifficultyBreakdownJson { get; set; }
}
