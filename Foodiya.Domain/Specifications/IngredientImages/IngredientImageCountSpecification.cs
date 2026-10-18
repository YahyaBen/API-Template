using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.IngredientImages;

/// <summary>
/// Specification: Count ingredient images using the same filters as IngredientImageListSpecification.
/// </summary>
public sealed class IngredientImageCountSpecification : BaseSpecification<IngredientImage>
{
    public IngredientImageCountSpecification(
        int? ingredientId = null,
        bool? isPrimary = null,
        string? search = null)
        : base(image =>
            (!ingredientId.HasValue || image.IngredientId == ingredientId.Value)
            && (!isPrimary.HasValue || image.IsPrimary == isPrimary.Value)
            && (string.IsNullOrWhiteSpace(search)
                || image.Ingredient.Name.ToLower().Contains(search.Trim().ToLower())
                || image.ImageUrl.ToLower().Contains(search.Trim().ToLower())
                || (image.AltText != null && image.AltText.ToLower().Contains(search.Trim().ToLower()))))
    {
    }
}
