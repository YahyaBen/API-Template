namespace Foodiya.Functions.Models;

// ── Daily Stats DTOs ─────────────────────────────────────────

/// <summary>
/// Snapshot of daily platform-wide recipe analytics.
/// Computed by the DailyRecipeStatsFunction timer trigger.
/// </summary>
public sealed class DailyRecipeStatsResult
{
    public DateTime GeneratedAtUtc { get; set; }

    // ── Totals ──────────────────────────────────
    public int TotalRecipes { get; set; }
    public int TotalPublishedRecipes { get; set; }
    public int NewRecipesToday { get; set; }
    public int TotalLikesToday { get; set; }

    // ── Top lists ───────────────────────────────
    public List<RecipeStatItem> TopLikedRecipes { get; set; } = [];
    public List<RecipeStatItem> RecentRecipes { get; set; } = [];
    public List<CuisineStatItem> TopCuisines { get; set; } = [];
    public List<DifficultyStatItem> RecipesByDifficulty { get; set; } = [];
}

public sealed class RecipeStatItem
{
    public int RecipeId { get; set; }
    public string Title { get; set; } = null!;
    public string ChefDisplayName { get; set; } = null!;
    public int LikeCount { get; set; }
    public DateTime PublishedAt { get; set; }
}

public sealed class CuisineStatItem
{
    public int CuisineId { get; set; }
    public string CuisineName { get; set; } = null!;
    public int RecipeCount { get; set; }
}

public sealed class DifficultyStatItem
{
    public string DifficultyName { get; set; } = null!;
    public int RecipeCount { get; set; }
}

// ── Welcome Email DTOs ───────────────────────────────────────

/// <summary>
/// Queue message payload sent by the API when a user registers.
/// </summary>
public sealed class WelcomeEmailMessage
{
    public int UserId { get; set; }
    public string Email { get; set; } = null!;
    public string FirstName { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public DateTime RegisteredAtUtc { get; set; }
}

/// <summary>
/// Log entry produced after processing a welcome email.
/// </summary>
public sealed class WelcomeEmailResult
{
    public int UserId { get; set; }
    public string Email { get; set; } = null!;
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime ProcessedAtUtc { get; set; }
}

// ── Nutrition Calculator DTOs ────────────────────────────────

/// <summary>
/// Single ingredient entry sent by the client for nutrition calculation.
/// </summary>
public sealed class NutritionIngredientInput
{
    /// <summary>Ingredient ID from the database.</summary>
    public int IngredientId { get; set; }

    /// <summary>Quantity in grams (normalized by the caller).</summary>
    public decimal QuantityGrams { get; set; }
}

/// <summary>
/// HTTP request body for the Nutrition Calculator function.
/// </summary>
public sealed class NutritionCalculatorRequest
{
    public int Servings { get; set; } = 1;
    public List<NutritionIngredientInput> Ingredients { get; set; } = [];
}

/// <summary>
/// Per-ingredient nutrition breakdown returned by the calculator.
/// </summary>
public sealed class NutritionIngredientDetail
{
    public int IngredientId { get; set; }
    public string IngredientName { get; set; } = null!;
    public decimal QuantityGrams { get; set; }
    public decimal Calories { get; set; }
}

/// <summary>
/// Full nutrition response from the calculator function.
/// </summary>
public sealed class NutritionCalculatorResponse
{
    public int Servings { get; set; }
    public decimal TotalCalories { get; set; }
    public decimal CaloriesPerServing { get; set; }
    public List<NutritionIngredientDetail> Breakdown { get; set; } = [];
}
