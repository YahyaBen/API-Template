using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.RecipeImages;

/// <summary>
/// Specification: Paginated recipe image list with optional recipe, primary flag, and free-text search filters.
/// </summary>
public sealed class RecipeImageListSpecification : BaseSpecification<RecipeImage>
{
    public RecipeImageListSpecification(
        int page,
        int pageSize,
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
        AddInclude(image => image.Recipe);

        ApplyOrderBy(image => image.SortOrder);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
