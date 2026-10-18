using AutoMapper;
using Foodiya.Application.DTOs.RecipeLike.Response;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Profiles;

public sealed class RecipeLikeProfile : Profile
{
    public RecipeLikeProfile()
    {
        CreateMap<RecipeLike, RecipeLikeDetailResponse>()
            .ForMember(d => d.RecipeUuid, o => o.MapFrom(s => s.Recipe.Uuid))
            .ForMember(d => d.UserUuid, o => o.MapFrom(s => s.User.Uuid))
            .ForMember(d => d.RecipeTitle, o => o.MapFrom(s => s.Recipe.Title))
            .ForMember(d => d.UserName, o => o.MapFrom(s => s.User.UserName))
            .ForMember(d => d.UserFullName, o => o.MapFrom(s => $"{s.User.FirstName} {s.User.LastName}".Trim()));
    }
}
