using Foodiya.Application.DTOs.MoroccanCity.Request;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Factories;

public interface IMoroccanCityFactory
{
    MoroccanCity Create(CreateMoroccanCityRequest request);
    void Update(MoroccanCity city, UpdateMoroccanCityRequest request, DateTime utcNow);
}
