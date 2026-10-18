using Foodiya.Application.DTOs.ChefProfile.Request;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Factories;

public interface IChefProfileFactory
{
    ChefProfile Create(int userId, CreateChefProfileRequest request);
    void Update(ChefProfile chefProfile, UpdateChefProfileRequest request);
}
