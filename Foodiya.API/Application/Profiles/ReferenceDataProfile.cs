using AutoMapper;
using Foodiya.Application.DTOs.ReferenceData.Response;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Profiles;

public sealed class ReferenceDataProfile : Profile
{
    public ReferenceDataProfile()
    {
        CreateMap<ReferenceDataItem, ReferenceDataItemResponse>();
        CreateMap<ReferenceData, ReferenceDataResponse>();
    }
}
