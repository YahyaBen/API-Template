using AutoMapper;
using Foodiya.Application.DTOs.IngredientType.Response;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Profiles;

public sealed class IngredientTypeProfile : Profile
{
    public IngredientTypeProfile()
    {
        CreateMap<IngredientType, IngredientTypeDetailResponse>();
    }
}
