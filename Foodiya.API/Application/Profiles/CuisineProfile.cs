using AutoMapper;
using Foodiya.Application.DTOs.Cuisine.Response;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Profiles;

public sealed class CuisineProfile : Profile
{
    public CuisineProfile()
    {
        CreateMap<Cuisine, CuisineDetailResponse>();
    }
}
