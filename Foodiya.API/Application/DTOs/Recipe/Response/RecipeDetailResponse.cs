namespace Foodiya.Application.DTOs.Recipe.Response;

public sealed class RecipeDetailResponse
{
    public Guid Uuid { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Summary { get; set; }
    public string? CoverImageUrl { get; set; }
    public int PrepTimeMinutes { get; set; }
    public int CookTimeMinutes { get; set; }
    public int TotalTimeMinutes { get; set; }
    public int Servings { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Visibility { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime DateInsert { get; set; }
    public DateTime? DateModif { get; set; }
    public DateTime? PublishedAt { get; set; }

    public string DifficultyName { get; set; } = string.Empty;
    public string? CuisineName { get; set; }
    public string? CityName { get; set; }
    public string ChefDisplayName { get; set; } = string.Empty;
    public Guid ChefUuid { get; set; }

    public int LikesCount { get; set; }
    public IReadOnlyCollection<string> Categories { get; set; } = [];
    public IReadOnlyCollection<RecipeStepResponse> Steps { get; set; } = [];
    public IReadOnlyCollection<RecipeIngredientResponse> Ingredients { get; set; } = [];
    public IReadOnlyCollection<RecipeImageResponse> Images { get; set; } = [];
    public RecipeNutritionResponse? Nutrition { get; set; }
}
