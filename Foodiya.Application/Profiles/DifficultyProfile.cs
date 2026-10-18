using AutoMapper;
using Foodiya.Application.DTOs.Difficulty.Response;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Profiles;

public sealed class DifficultyProfile : Profile
{
    public DifficultyProfile()
    {
        CreateMap<Difficulty, DifficultyDetailResponse>();
    }
}
