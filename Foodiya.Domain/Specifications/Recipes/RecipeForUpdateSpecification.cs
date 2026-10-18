using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.Recipes;

/// <summary>
/// Specification: Recipe for update — loads with tracking + child collections for mutation.
/// </summary>
public sealed class RecipeForUpdateSpecification : BaseSpecification<Recipe>
{
    public RecipeForUpdateSpecification(int id) : base(r => r.Id == id && r.DeletedAt == null)
    {
        AddInclude(r => r.Difficulty);
        AddInclude(r => r.Cuisine!);
        AddInclude(r => r.City!);
        AddInclude(r => r.FoodCategories);
        AddInclude(r => r.RecipeSteps);
        AddInclude(r => r.RecipeIngredients);
        ApplyTracking();
    }
}
