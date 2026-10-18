using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.RecipeIngredients;

/// <summary>
/// Specification: Recipe ingredient detail by composite key with linked recipe, ingredient, and unit.
/// </summary>
public sealed class RecipeIngredientByIdSpecification : BaseSpecification<RecipeIngredient>
{
    public RecipeIngredientByIdSpecification(int recipeId, int ingredientId)
        : base(recipeIngredient =>
            recipeIngredient.RecipeId == recipeId
            && recipeIngredient.IngredientId == ingredientId
            && recipeIngredient.Recipe.DeletedAt == null)
    {
        AddInclude(recipeIngredient => recipeIngredient.Recipe);
        AddInclude(recipeIngredient => recipeIngredient.Ingredient);
        AddInclude(recipeIngredient => recipeIngredient.Unit!);
    }
}
