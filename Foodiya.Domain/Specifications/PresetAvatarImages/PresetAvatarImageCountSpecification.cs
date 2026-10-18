using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.PresetAvatarImages;

/// <summary>
/// Specification: Count preset avatar images using the same filters as PresetAvatarImageListSpecification.
/// </summary>
public sealed class PresetAvatarImageCountSpecification : BaseSpecification<PresetAvatarImage>
{
    public PresetAvatarImageCountSpecification(
        bool? isActive = null,
        string? search = null)
        : base(image =>
            (!isActive.HasValue || image.IsActive == isActive.Value)
            && (string.IsNullOrWhiteSpace(search)
                || image.Code.ToLower().Contains(search.Trim().ToLower())
                || image.Label.ToLower().Contains(search.Trim().ToLower())
                || (image.BackgroundColor != null && image.BackgroundColor.ToLower().Contains(search.Trim().ToLower()))))
    {
    }
}
