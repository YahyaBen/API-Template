using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.RecipeIngredients;

/// <summary>
/// Specification: Paginated recipe ingredient list with optional filters by recipe, ingredient, unit, and free-text search.
/// </summary>
public sealed class RecipeIngredientListSpecification : BaseSpecification<RecipeIngredient>
{
    public RecipeIngredientListSpecification(
        int page,
        int pageSize,
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
        AddInclude(recipeIngredient => recipeIngredient.Recipe);
        AddInclude(recipeIngredient => recipeIngredient.Ingredient);
        AddInclude(recipeIngredient => recipeIngredient.Unit!);

        ApplyOrderBy(recipeIngredient => recipeIngredient.SortOrder);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
