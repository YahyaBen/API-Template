using Foodiya.Application.DTOs.ChefProfile.Request;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Domain.Extensions;
using Foodiya.Domain.Models;
using static Foodiya.Application.Factories.Helpers.EntityNormalizationHelper;

namespace Foodiya.Application.Factories;

public sealed class ChefProfileFactory : IChefProfileFactory
{
    public ChefProfile Create(int userId, CreateChefProfileRequest request) => new()
    {
        UserId = userId,
        DisplayName = Required(request.DisplayName, nameof(request.DisplayName)),
        Bio = Optional(request.Bio),
        Specialty = Optional(request.Specialty),
        YearsOfExperience = request.YearsOfExperience,
        IsVerified = false,
        Code = EntityCodeGenerator.For("CHF")
    };

    public void Update(ChefProfile chefProfile, UpdateChefProfileRequest request)
    {
        if (request.DisplayName is not null)
            chefProfile.DisplayName = Required(request.DisplayName, nameof(request.DisplayName));

        if (request.Bio is not null)
            chefProfile.Bio = Optional(request.Bio);

        if (request.Specialty is not null)
            chefProfile.Specialty = Optional(request.Specialty);

        if (request.YearsOfExperience.HasValue)
            chefProfile.YearsOfExperience = request.YearsOfExperience.Value;
    }
}
