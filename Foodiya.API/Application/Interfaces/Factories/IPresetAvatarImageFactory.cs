using Foodiya.Application.DTOs.PresetAvatarImage.Request;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Factories;

public interface IPresetAvatarImageFactory
{
    PresetAvatarImage Create(CreatePresetAvatarImageRequest request);
    void Update(PresetAvatarImage image, UpdatePresetAvatarImageRequest request);
}
