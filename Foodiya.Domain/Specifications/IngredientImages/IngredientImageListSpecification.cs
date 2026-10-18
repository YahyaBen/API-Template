using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.IngredientImages;

/// <summary>
/// Specification: Paginated ingredient image list with optional ingredient, primary flag, and free-text search filters.
/// </summary>
public sealed class IngredientImageListSpecification : BaseSpecification<IngredientImage>
{
    public IngredientImageListSpecification(
        int page,
        int pageSize,
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
        AddInclude(image => image.Ingredient);

        ApplyOrderBy(image => image.SortOrder);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
