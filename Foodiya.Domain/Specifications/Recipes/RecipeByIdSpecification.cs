using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.Recipes;

/// <summary>
/// Specification: Recipe detail by ID — eager-loads all related data.
/// </summary>
public sealed class RecipeByIdSpecification : BaseSpecification<Recipe>
{
    public RecipeByIdSpecification(int id, bool includeInactive = false)
        : base(r => r.Id == id && r.DeletedAt == null && (includeInactive || r.IsActive))
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
    }
}
