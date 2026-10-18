using AutoMapper;
using Foodiya.Application.DTOs.MoroccanRegion.Response;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Profiles;

public sealed class MoroccanRegionProfile : Profile
{
    public MoroccanRegionProfile()
    {
        CreateMap<MoroccanRegion, MoroccanRegionDetailResponse>();
    }
}
