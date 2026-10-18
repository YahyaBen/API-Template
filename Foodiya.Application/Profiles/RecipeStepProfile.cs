using AutoMapper;
using Foodiya.Application.DTOs.RecipeStep.Response;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Profiles;

public sealed class RecipeStepProfile : Profile
{
    public RecipeStepProfile()
    {
        CreateMap<RecipeStep, RecipeStepDetailResponse>()
            .ForMember(d => d.RecipeUuid, o => o.MapFrom(s => s.Recipe.Uuid))
            .ForMember(d => d.RecipeTitle, o => o.MapFrom(s => s.Recipe.Title));
    }
}
