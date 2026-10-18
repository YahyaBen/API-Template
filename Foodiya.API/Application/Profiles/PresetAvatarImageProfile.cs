using AutoMapper;
using Foodiya.Application.DTOs.PresetAvatarImage.Response;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Profiles;

public sealed class PresetAvatarImageProfile : Profile
{
    public PresetAvatarImageProfile()
    {
        CreateMap<PresetAvatarImage, PresetAvatarImageDetailResponse>();
    }
}
