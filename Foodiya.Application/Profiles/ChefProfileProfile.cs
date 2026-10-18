using AutoMapper;
using Foodiya.Application.DTOs.ChefProfile.Response;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Profiles;

public sealed class ChefProfileProfile : Profile
{
    public ChefProfileProfile()
    {
        CreateMap<ChefProfile, ChefProfileDetailResponse>()
            .ForMember(d => d.UserUuid, o => o.MapFrom(s => s.User.Uuid))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.UserName))
            .ForMember(d => d.CodeAlpha, o => o.MapFrom(s => $"{s.User.Code}"))
            .ForMember(d => d.FirstName, o => o.MapFrom(s => s.User.FirstName))
            .ForMember(d => d.LastName, o => o.MapFrom(s => s.User.LastName))
            .ForMember(d => d.Email, o => o.MapFrom(s => s.User.Email))
            .ForMember(d => d.ProfileImageUrl, o => o.MapFrom(s => s.User.ProfileImageUrl))
            .ForMember(d => d.Color, o => o.MapFrom(s => s.User.Color))
            .ForMember(d => d.FullName, o => o.MapFrom(s => $"{s.User.FirstName} {s.User.LastName}"));
    }
}
