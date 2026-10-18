using AutoMapper;
using Foodiya.Application.DTOs.Unit.Response;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Profiles;

public sealed class UnitProfile : Profile
{
    public UnitProfile()
    {
        CreateMap<Unit, UnitDetailResponse>();
    }
}
