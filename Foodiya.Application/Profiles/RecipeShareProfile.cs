using AutoMapper;
using Foodiya.Application.DTOs.RecipeShare.Response;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Profiles;

public sealed class RecipeShareProfile : Profile
{
    public RecipeShareProfile()
    {
        CreateMap<RecipeShare, RecipeShareDetailResponse>()
            .ForMember(d => d.RecipeUuid, o => o.MapFrom(s => s.Recipe.Uuid))
            .ForMember(d => d.SharedByUserUuid, o => o.MapFrom(s => s.SharedByUser.Uuid))
            .ForMember(d => d.SharedWithUserUuid, o => o.MapFrom(s => s.SharedWithUser != null ? s.SharedWithUser.Uuid : (Guid?)null))
            .ForMember(d => d.RecipeTitle, o => o.MapFrom(s => s.Recipe.Title))
            .ForMember(d => d.SharedByUserName, o => o.MapFrom(s => s.SharedByUser.UserName))
            .ForMember(d => d.SharedByUserFullName, o => o.MapFrom(s => $"{s.SharedByUser.FirstName} {s.SharedByUser.LastName}".Trim()))
            .ForMember(d => d.SharedWithUserName, o => o.MapFrom(s => s.SharedWithUser != null ? s.SharedWithUser.UserName : null))
            .ForMember(d => d.SharedWithUserFullName, o => o.MapFrom(s => s.SharedWithUser != null ? $"{s.SharedWithUser.FirstName} {s.SharedWithUser.LastName}".Trim() : null));
    }
}
