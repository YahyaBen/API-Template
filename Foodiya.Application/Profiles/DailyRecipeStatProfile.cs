using AutoMapper;
using Foodiya.Application.DTOs.DailyRecipeStat.Response;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Profiles;

public sealed class DailyRecipeStatProfile : Profile
{
    public DailyRecipeStatProfile()
    {
        CreateMap<DailyRecipeStat, DailyRecipeStatDetailResponse>();
    }
}
