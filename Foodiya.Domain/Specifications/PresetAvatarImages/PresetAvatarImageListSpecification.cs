using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.PresetAvatarImages;

/// <summary>
/// Specification: Paginated preset avatar image list with optional active filter and free-text search.
/// </summary>
public sealed class PresetAvatarImageListSpecification : BaseSpecification<PresetAvatarImage>
{
    public PresetAvatarImageListSpecification(
        int page,
        int pageSize,
        bool? isActive = null,
        string? search = null)
        : base(image =>
            (!isActive.HasValue || image.IsActive == isActive.Value)
            && (string.IsNullOrWhiteSpace(search)
                || image.Code.ToLower().Contains(search.Trim().ToLower())
                || image.Label.ToLower().Contains(search.Trim().ToLower())
                || (image.BackgroundColor != null && image.BackgroundColor.ToLower().Contains(search.Trim().ToLower()))))
    {
        ApplyOrderBy(image => image.SortOrder);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
