using Foodiya.Application.DTOs.PresetAvatarImage.Request;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Domain.Exceptions;
using Foodiya.Domain.Models;
using static Foodiya.Application.Factories.Helpers.EntityNormalizationHelper;

namespace Foodiya.Application.Factories;

public sealed class PresetAvatarImageFactory : IPresetAvatarImageFactory
{
    public PresetAvatarImage Create(CreatePresetAvatarImageRequest request) => new()
    {
        Code = Code(request.Code, nameof(request.Code)),
        Label = Required(request.Label, nameof(request.Label)),
        ImageUrl = Required(request.ImageUrl, nameof(request.ImageUrl)),
        BackgroundColor = Optional(request.BackgroundColor),
        SortOrder = request.SortOrder,
        IsActive = request.IsActive
    };

    public void Update(PresetAvatarImage image, UpdatePresetAvatarImageRequest request)
    {
        if (request.ClearBackgroundColor && request.BackgroundColor is not null)
            throw new FoodiyaBadRequestException("Provide BackgroundColor or ClearBackgroundColor, not both.");

        if (request.Code is not null)
            image.Code = Code(request.Code, nameof(request.Code));

        if (request.Label is not null)
            image.Label = Required(request.Label, nameof(request.Label));

        if (request.ImageUrl is not null)
            image.ImageUrl = Required(request.ImageUrl, nameof(request.ImageUrl));

        if (request.ClearBackgroundColor)
            image.BackgroundColor = null;
        else if (request.BackgroundColor is not null)
            image.BackgroundColor = Optional(request.BackgroundColor);

        if (request.SortOrder.HasValue)
            image.SortOrder = request.SortOrder.Value;

        if (request.IsActive.HasValue)
            image.IsActive = request.IsActive.Value;
    }
}
