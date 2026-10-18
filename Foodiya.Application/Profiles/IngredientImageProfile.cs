using AutoMapper;
using Foodiya.Application.DTOs.IngredientImage.Response;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Profiles;

public sealed class IngredientImageProfile : Profile
{
    public IngredientImageProfile()
    {
        CreateMap<IngredientImage, IngredientImageDetailResponse>()
            .ForMember(d => d.IngredientUuid, o => o.MapFrom(s => s.Ingredient.Uuid))
            .ForMember(d => d.IngredientName, o => o.MapFrom(s => s.Ingredient.Name));
    }
}
