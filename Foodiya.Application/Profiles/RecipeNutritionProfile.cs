using AutoMapper;
using Foodiya.Application.DTOs.RecipeNutrition.Response;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Profiles;

public sealed class RecipeNutritionProfile : Profile
{
    public RecipeNutritionProfile()
    {
        CreateMap<RecipeNutrition, RecipeNutritionDetailResponse>()
            .ForMember(d => d.RecipeUuid, o => o.MapFrom(s => s.Recipe.Uuid))
            .ForMember(d => d.Code, o => o.MapFrom(s => s.Uuid.ToString()))
            .ForMember(d => d.RecipeTitle, o => o.MapFrom(s => s.Recipe.Title));
    }
}
