using Foodiya.Application.DTOs.MoroccanCity.Request;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Domain.Extensions;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using static Foodiya.Application.Factories.Helpers.EntityNormalizationHelper;

namespace Foodiya.Application.Factories;

public sealed class MoroccanCityFactory : IMoroccanCityFactory
{
    public MoroccanCity Create(CreateMoroccanCityRequest request) => new()
    {
        RegionId = request.RegionId,
        Name = Required(request.Name, nameof(request.Name)),
        Slug = Slug(request.Name),
        SortOrder = request.SortOrder,
        IsActive = request.IsActive,
        Code = EntityCodeGenerator.FromSlug(Slug(request.Name))
    };

    public void Update(MoroccanCity city, UpdateMoroccanCityRequest request, DateTime utcNow)
    {
        if (request.RegionId.HasValue)
            city.RegionId = request.RegionId.Value;

        if (request.Name is not null)
        {
            city.Name = Required(request.Name, nameof(request.Name));
            city.Slug = Slug(request.Name);
        }

        if (request.SortOrder.HasValue)
            city.SortOrder = request.SortOrder.Value;

        if (request.IsActive.HasValue)
            city.IsActive = request.IsActive.Value;

        city.DateModif = utcNow;
    }
}
