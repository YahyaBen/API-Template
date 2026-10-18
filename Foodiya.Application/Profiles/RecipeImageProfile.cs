using AutoMapper;
using Foodiya.Application.DTOs.RecipeImage.Response;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Profiles;

public sealed class RecipeImageProfile : Profile
{
    public RecipeImageProfile()
    {
        CreateMap<RecipeImage, RecipeImageDetailResponse>()
            .ForMember(d => d.RecipeUuid, o => o.MapFrom(s => s.Recipe.Uuid))
            .ForMember(d => d.RecipeTitle, o => o.MapFrom(s => s.Recipe.Title));
    }
}
