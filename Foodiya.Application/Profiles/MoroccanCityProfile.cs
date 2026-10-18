using AutoMapper;
using Foodiya.Application.DTOs.MoroccanCity.Response;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Profiles;

public sealed class MoroccanCityProfile : Profile
{
    public MoroccanCityProfile()
    {
        CreateMap<MoroccanCity, MoroccanCityDetailResponse>()
            .ForMember(d => d.RegionUuid, o => o.MapFrom(s => s.Region.Uuid))
            .ForMember(d => d.RegionName, o => o.MapFrom(s => s.Region.Name));
    }
}
