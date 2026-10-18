using Foodiya.Domain.Constants;
using Foodiya.Application.DTOs.Recipe.Request;
using Foodiya.Domain.Extensions;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
// EntityCodeGenerator is in Foodiya.Domain.Extensions (already imported)

namespace Foodiya.Application.Factories;

/// <summary>
/// Single Responsibility: builds and mutates <see cref="Recipe"/> entities from DTOs.
/// Keeps construction logic (slug generation, child collections, status normalization)
/// out of the service layer.
/// </summary>
public sealed class RecipeFactory : IRecipeFactory
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public RecipeFactory(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    /// <summary>
    /// Build a brand-new <see cref="Recipe"/> entity from a create request.
    /// </summary>
    public Recipe Create(int chefId, CreateRecipeRequest request)
    {
        var recipe = new Recipe
        {
            ChefId = chefId,
            Title = request.Title,
            Slug = GenerateSlug(request.Title),
            Summary = request.Summary,
            DifficultyId = request.DifficultyId,
            CuisineId = request.CuisineId,
            CityId = request.CityId,
            PrepTimeMinutes = request.PrepTimeMinutes,
            CookTimeMinutes = request.CookTimeMinutes,
            Servings = request.Servings,
            CoverImageUrl = request.CoverImageUrl,
            Status = NormalizeStatus(request.Status),
            Visibility = RecipeVisibilityConstants.Public,
            IsActive = request.IsActive,
            DateInsert = _dateTimeProvider.UtcNow,
            Code = EntityCodeGenerator.For("RCP")
        };

        AddSteps(recipe, request.Steps);
        AddIngredients(recipe, request.Ingredients);

        return recipe;
    }

    /// <summary>
    /// Apply an update request onto an existing tracked <see cref="Recipe"/> entity.
    /// </summary>
    public void Update(Recipe recipe, UpdateRecipeRequest request)
    {
        if (request.Title is not null)
        {
            recipe.Title = request.Title;
            recipe.Slug = GenerateSlug(request.Title);
        }

        if (request.Summary is not null)
            recipe.Summary = request.Summary;

        if (request.DifficultyId.HasValue)
            recipe.DifficultyId = request.DifficultyId.Value;

        if (request.CuisineId.HasValue)
            recipe.CuisineId = request.CuisineId;

        if (request.CityId.HasValue)
            recipe.CityId = request.CityId;

        if (request.PrepTimeMinutes.HasValue)
            recipe.PrepTimeMinutes = request.PrepTimeMinutes.Value;

        if (request.CookTimeMinutes.HasValue)
            recipe.CookTimeMinutes = request.CookTimeMinutes.Value;

        if (request.Servings.HasValue)
            recipe.Servings = request.Servings.Value;

        if (request.CoverImageUrl is not null)
            recipe.CoverImageUrl = request.CoverImageUrl;

        if (!string.IsNullOrWhiteSpace(request.Status))
            recipe.Status = NormalizeStatus(request.Status);

        if (!string.IsNullOrWhiteSpace(request.Visibility))
            recipe.Visibility = NormalizeVisibility(request.Visibility);

        if (request.IsActive.HasValue)
            recipe.IsActive = request.IsActive.Value;

        recipe.DateModif = _dateTimeProvider.UtcNow;

        // Replace child collections only if provided
        if (request.Steps is not null)
        {
            recipe.RecipeSteps.Clear();
            AddSteps(recipe, request.Steps);
        }

        if (request.Ingredients is not null)
        {
            recipe.RecipeIngredients.Clear();
            AddIngredients(recipe, request.Ingredients);
        }
    }

    // ─── Private helpers ────────────────────────────────────────

    private static string GenerateSlug(string title)
        => title.ToSlug() + "-" + Guid.NewGuid().ToString("N")[..8];

    private static string NormalizeStatus(string status) => status.Trim().ToUpperInvariant() switch
    {
        RecipeStatusConstants.Published => RecipeStatusConstants.Published,
        RecipeStatusConstants.Archived => RecipeStatusConstants.Archived,
        _ => RecipeStatusConstants.Draft
    };

    private static string NormalizeVisibility(string visibility) => visibility.Trim().ToUpperInvariant() switch
    {
        RecipeVisibilityConstants.Private => RecipeVisibilityConstants.Private,
        RecipeVisibilityConstants.Unlisted => RecipeVisibilityConstants.Unlisted,
        _ => RecipeVisibilityConstants.Public
    };

    private static void AddSteps(Recipe recipe, ICollection<CreateRecipeStepRequest> steps)
    {
        foreach (var dto in steps)
        {
            recipe.RecipeSteps.Add(new RecipeStep
            {
                StepNumber = dto.StepNumber,
                Title = dto.Title,
                Instruction = dto.Instruction,
                DurationMinutes = dto.DurationMinutes,
                Code = EntityCodeGenerator.For("RST")
            });
        }
    }

    private static void AddIngredients(Recipe recipe, ICollection<CreateRecipeIngredientItem> ingredients)
    {
        foreach (var dto in ingredients)
        {
            recipe.RecipeIngredients.Add(new RecipeIngredient
            {
                IngredientId = dto.IngredientId,
                Quantity = dto.Quantity,
                UnitId = dto.UnitId,
                IsOptional = dto.IsOptional,
                Notes = dto.Notes,
                SortOrder = dto.SortOrder,
                Code = EntityCodeGenerator.For("RIG")
            });
        }
    }
}
