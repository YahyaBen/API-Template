using Foodiya.Application.DTOs.Unit.Request;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Factories;

public interface IUnitFactory
{
    Unit Create(CreateUnitRequest request);
    void Update(Unit unit, UpdateUnitRequest request, DateTime utcNow);
}
