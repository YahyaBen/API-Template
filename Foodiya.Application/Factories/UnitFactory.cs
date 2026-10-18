using Foodiya.Application.DTOs.Unit.Request;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using static Foodiya.Application.Factories.Helpers.EntityNormalizationHelper;

namespace Foodiya.Application.Factories;

public sealed class UnitFactory : IUnitFactory
{
    public Unit Create(CreateUnitRequest request) => new()
    {
        Code = Code(request.Code, nameof(request.Code)),
        Label = Required(request.Label, nameof(request.Label)),
        SortOrder = request.SortOrder,
        IsActive = request.IsActive
    };

    public void Update(Unit unit, UpdateUnitRequest request, DateTime utcNow)
    {
        if (request.Code is not null)
            unit.Code = Code(request.Code, nameof(request.Code));

        if (request.Label is not null)
            unit.Label = Required(request.Label, nameof(request.Label));

        if (request.SortOrder.HasValue)
            unit.SortOrder = request.SortOrder.Value;

        if (request.IsActive.HasValue)
            unit.IsActive = request.IsActive.Value;

        unit.DateModif = utcNow;
    }
}
