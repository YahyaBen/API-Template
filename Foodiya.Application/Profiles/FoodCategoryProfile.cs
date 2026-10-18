using AutoMapper;
using Foodiya.Application.DTOs.FoodCategory.Response;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Profiles;

public sealed class FoodCategoryProfile : Profile
{
    public FoodCategoryProfile()
    {
        CreateMap<FoodCategory, FoodCategoryDetailResponse>();
    }
}
