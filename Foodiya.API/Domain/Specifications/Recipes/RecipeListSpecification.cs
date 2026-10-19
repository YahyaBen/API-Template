using Foodiya.Domain.Constants;
using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.Recipes;

/// <summary>
/// Specification: Paginated recipe list with optional filters for cuisine, difficulty, category, and free-text search.
/// </summary>
public sealed class RecipeListSpecification : BaseSpecification<Recipe>
{
    public RecipeListSpecification(
        int page,
        int pageSize,
        int? cuisineId = null,
        int? difficultyId = null,
        int? categoryId = null,
        string? search = null)
        : base(r =>
            r.DeletedAt == null
            && r.Status == RecipeStatusConstants.Published
            && r.IsActive
            && (!cuisineId.HasValue || r.CuisineId == cuisineId.Value)
            && (!difficultyId.HasValue || r.DifficultyId == difficultyId.Value)
            && (!categoryId.HasValue || r.FoodCategories.Any(c => c.Id == categoryId.Value))
            && (string.IsNullOrWhiteSpace(search)
                || r.Title.ToLower().Contains(search.Trim().ToLower())
                || (r.Summary != null && r.Summary.ToLower().Contains(search.Trim().ToLower()))))
    {
        AddInclude(r => r.Difficulty);
        AddInclude(r => r.Cuisine!);
        AddInclude(r => r.City!);
        AddInclude(r => r.Chef);
        AddInclude(r => r.RecipeNutrition!);
        AddInclude(r => r.FoodCategories);
        AddInclude(r => r.RecipeSteps);
        AddInclude(r => r.RecipeImages);
        AddInclude(r => r.RecipeIngredients)
                         .ThenInclude(ri => ri.Ingredient);
        AddInclude(r => r.RecipeIngredients)
                         .ThenInclude(ri => ri.Unit!);
        AddInclude(r => r.RecipeLikes);

        ApplyOrderByDescending(r => r.PublishedAt!);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
