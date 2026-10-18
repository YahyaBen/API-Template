using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.PresetAvatarImages;

/// <summary>
/// Specification: Preset avatar image detail by ID.
/// </summary>
public sealed class PresetAvatarImageByIdSpecification : BaseSpecification<PresetAvatarImage>
{
    public PresetAvatarImageByIdSpecification(int id) : base(image => image.Id == id)
    {
    }
}
