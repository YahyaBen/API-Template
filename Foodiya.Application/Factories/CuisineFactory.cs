using Foodiya.Application.DTOs.Cuisine.Request;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using static Foodiya.Application.Factories.Helpers.EntityNormalizationHelper;

namespace Foodiya.Application.Factories;

public sealed class CuisineFactory : ICuisineFactory
{
    public Cuisine Create(CreateCuisineRequest request) => new()
    {
        Name = Required(request.Name, nameof(request.Name)),
        Code = Code(request.Code, nameof(request.Code)),
        SortOrder = request.SortOrder,
        IconUrl = Optional(request.IconUrl),
        Color = Optional(request.Color),
        IsActive = request.IsActive
    };

    public void Update(Cuisine cuisine, UpdateCuisineRequest request, DateTime utcNow)
    {
        if (request.Name is not null)
            cuisine.Name = Required(request.Name, nameof(request.Name));

        if (request.Code is not null)
            cuisine.Code = Code(request.Code, nameof(request.Code));

        if (request.SortOrder.HasValue)
            cuisine.SortOrder = request.SortOrder.Value;

        if (request.IconUrl is not null)
            cuisine.IconUrl = Optional(request.IconUrl);

        if (request.Color is not null)
            cuisine.Color = Optional(request.Color);

        if (request.IsActive.HasValue)
            cuisine.IsActive = request.IsActive.Value;

        cuisine.DateModif = utcNow;
    }
}
