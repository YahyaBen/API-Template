using Foodiya.Application.DTOs.MoroccanRegion.Request;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Factories;

public interface IMoroccanRegionFactory
{
    MoroccanRegion Create(CreateMoroccanRegionRequest request);
    void Update(MoroccanRegion region, UpdateMoroccanRegionRequest request, DateTime utcNow);
}
