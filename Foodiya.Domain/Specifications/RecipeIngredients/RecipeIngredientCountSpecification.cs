using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.RecipeIngredients;

/// <summary>
/// Specification: Count recipe ingredients using the same filters as RecipeIngredientListSpecification.
/// </summary>
public sealed class RecipeIngredientCountSpecification : BaseSpecification<RecipeIngredient>
{
    public RecipeIngredientCountSpecification(
        int? recipeId = null,
        int? ingredientId = null,
        int? unitId = null,
        string? search = null)
        : base(recipeIngredient =>
            recipeIngredient.Recipe.DeletedAt == null
            && (!recipeId.HasValue || recipeIngredient.RecipeId == recipeId.Value)
            && (!ingredientId.HasValue || recipeIngredient.IngredientId == ingredientId.Value)
            && (!unitId.HasValue || recipeIngredient.UnitId == unitId.Value)
            && (string.IsNullOrWhiteSpace(search)
                || recipeIngredient.Recipe.Title.ToLower().Contains(search.Trim().ToLower())
                || recipeIngredient.Ingredient.Name.ToLower().Contains(search.Trim().ToLower())
                || (recipeIngredient.Unit != null && recipeIngredient.Unit.Label.ToLower().Contains(search.Trim().ToLower()))
                || (recipeIngredient.Notes != null && recipeIngredient.Notes.ToLower().Contains(search.Trim().ToLower()))))
    {
    }
}
