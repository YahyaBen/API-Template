using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.Recipes;

/// <summary>
/// Specification: Recipe for delete — loads with tracking + ALL child collections
/// so EF can cascade-delete them in the correct order.
/// </summary>
public sealed class RecipeForDeleteSpecification : BaseSpecification<Recipe>
{
    public RecipeForDeleteSpecification(int id) : base(r => r.Id == id && r.DeletedAt == null)
    {
        AddInclude(r => r.FoodCategories);
        AddInclude(r => r.RecipeSteps);
        AddInclude(r => r.RecipeIngredients);
        AddInclude(r => r.RecipeImages);
        AddInclude(r => r.RecipeLikes);
        AddInclude(r => r.RecipeShares);
        AddInclude(r => r.RecipeNutrition!);
        ApplyTracking();
    }
}
