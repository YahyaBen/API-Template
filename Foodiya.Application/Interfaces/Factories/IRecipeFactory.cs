using Foodiya.Application.DTOs.Recipe.Request;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Factories;

/// <summary>
/// Port for building and mutating <see cref="Recipe"/> entities from DTOs.
/// </summary>
public interface IRecipeFactory
{
    /// <summary>
    /// Build a brand-new <see cref="Recipe"/> entity from a create request.
    /// </summary>
    Recipe Create(int chefId, CreateRecipeRequest request);

    /// <summary>
    /// Apply an update request onto an existing tracked <see cref="Recipe"/> entity.
    /// </summary>
    void Update(Recipe recipe, UpdateRecipeRequest request);
}
