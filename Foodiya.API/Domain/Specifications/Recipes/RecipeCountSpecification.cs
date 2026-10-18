using Foodiya.Domain.Constants;
using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.Recipes;

/// <summary>
/// Specification: Count published recipes matching the same filters as RecipeListSpecification.
/// Used for pagination metadata (no paging, no includes — just count).
/// </summary>
public sealed class RecipeCountSpecification : BaseSpecification<Recipe>
{
    public RecipeCountSpecification(
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
    }
}
