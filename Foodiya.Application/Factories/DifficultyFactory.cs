using Foodiya.Application.DTOs.Difficulty.Request;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using static Foodiya.Application.Factories.Helpers.EntityNormalizationHelper;

namespace Foodiya.Application.Factories;

public sealed class DifficultyFactory : IDifficultyFactory
{
    public Difficulty Create(CreateDifficultyRequest request) => new()
    {
        Name = Required(request.Name, nameof(request.Name)),
        Code = Code(request.Code, nameof(request.Code)),
        SortOrder = request.SortOrder,
        IconUrl = Optional(request.IconUrl),
        Color = Optional(request.Color),
        IsActive = request.IsActive
    };

    public void Update(Difficulty difficulty, UpdateDifficultyRequest request, DateTime utcNow)
    {
        if (request.Name is not null)
            difficulty.Name = Required(request.Name, nameof(request.Name));

        if (request.Code is not null)
            difficulty.Code = Code(request.Code, nameof(request.Code));

        if (request.SortOrder.HasValue)
            difficulty.SortOrder = request.SortOrder.Value;

        if (request.IconUrl is not null)
            difficulty.IconUrl = Optional(request.IconUrl);

        if (request.Color is not null)
            difficulty.Color = Optional(request.Color);

        if (request.IsActive.HasValue)
            difficulty.IsActive = request.IsActive.Value;

        difficulty.DateModif = utcNow;
    }
}
