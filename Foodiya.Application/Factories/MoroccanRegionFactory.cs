using Foodiya.Application.DTOs.MoroccanRegion.Request;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using static Foodiya.Application.Factories.Helpers.EntityNormalizationHelper;

namespace Foodiya.Application.Factories;

public sealed class MoroccanRegionFactory : IMoroccanRegionFactory
{
    public MoroccanRegion Create(CreateMoroccanRegionRequest request) => new()
    {
        Code = Code(request.Code, nameof(request.Code)),
        Name = Required(request.Name, nameof(request.Name)),
        SortOrder = request.SortOrder,
        IsActive = request.IsActive
    };

    public void Update(MoroccanRegion region, UpdateMoroccanRegionRequest request, DateTime utcNow)
    {
        if (request.Code is not null)
            region.Code = Code(request.Code, nameof(request.Code));

        if (request.Name is not null)
            region.Name = Required(request.Name, nameof(request.Name));

        if (request.SortOrder.HasValue)
            region.SortOrder = request.SortOrder.Value;

        if (request.IsActive.HasValue)
            region.IsActive = request.IsActive.Value;

        region.DateModif = utcNow;
    }
}
