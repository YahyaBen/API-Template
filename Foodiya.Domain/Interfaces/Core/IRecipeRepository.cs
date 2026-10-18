using Foodiya.Domain.Models;

namespace Foodiya.Domain.Interfaces.Core;

/// <summary>
/// Specialized repository for Recipe entities.
/// Extends <see cref="IGenericRepository{Recipe}"/> with domain-specific query methods.
/// </summary>
public interface IRecipeRepository : IGenericRepository<Recipe>
{
    /// <summary>
    /// Gets a recipe with all its relations eager-loaded (full detail view).
    /// </summary>
    Task<Recipe?> GetRecipeWithRelationsAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Gets a recipe with tracking + mutable child collections for update operations.
    /// </summary>
    Task<Recipe?> GetRecipeForUpdateAsync(int id, CancellationToken ct = default);

    /// <summary>
    /// Searches recipes by title or summary (case-insensitive partial match).
    /// </summary>
    Task<IReadOnlyList<Recipe>> SearchRecipesAsync(string searchTerm, CancellationToken ct = default);

    /// <summary>
    /// Gets all recipes belonging to a specific chef.
    /// </summary>
    Task<IReadOnlyList<Recipe>> GetRecipesByChefAsync(int chefId, CancellationToken ct = default);

    /// <summary>
    /// Gets all recipes filtered by cuisine.
    /// </summary>
    Task<IReadOnlyList<Recipe>> GetRecipesByCuisineAsync(int cuisineId, CancellationToken ct = default);

    /// <summary>
    /// Gets all recipes filtered by difficulty.
    /// </summary>
    Task<IReadOnlyList<Recipe>> GetRecipesByDifficultyAsync(int difficultyId, CancellationToken ct = default);

    /// <summary>
    /// Gets all recipes filtered by food category.
    /// </summary>
    Task<IReadOnlyList<Recipe>> GetRecipesByCategoryAsync(int categoryId, CancellationToken ct = default);
}
