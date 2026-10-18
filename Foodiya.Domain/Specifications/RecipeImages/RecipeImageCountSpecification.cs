using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.RecipeImages;

/// <summary>
/// Specification: Count recipe images using the same filters as RecipeImageListSpecification.
/// </summary>
public sealed class RecipeImageCountSpecification : BaseSpecification<RecipeImage>
{
    public RecipeImageCountSpecification(
        int? recipeId = null,
        bool? isPrimary = null,
        string? search = null)
        : base(image =>
            image.Recipe.DeletedAt == null
            && (!recipeId.HasValue || image.RecipeId == recipeId.Value)
            && (!isPrimary.HasValue || image.IsPrimary == isPrimary.Value)
            && (string.IsNullOrWhiteSpace(search)
                || image.Recipe.Title.ToLower().Contains(search.Trim().ToLower())
                || image.ImageUrl.ToLower().Contains(search.Trim().ToLower())
                || (image.AltText != null && image.AltText.ToLower().Contains(search.Trim().ToLower()))))
    {
    }
}
