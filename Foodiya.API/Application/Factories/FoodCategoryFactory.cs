using Foodiya.Application.DTOs.FoodCategory.Request;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Domain.Interfaces.Core;
using Foodiya.Domain.Models;
using static Foodiya.Application.Factories.Helpers.EntityNormalizationHelper;

namespace Foodiya.Application.Factories;

public sealed class FoodCategoryFactory : IFoodCategoryFactory
{
    public FoodCategory Create(CreateFoodCategoryRequest request) => new()
    {
        Name = Required(request.Name, nameof(request.Name)),
        Description = Optional(request.Description),
        Code = Code(request.Code, nameof(request.Code)),
        SortOrder = request.SortOrder,
        IconUrl = Optional(request.IconUrl),
        Color = Optional(request.Color),
        IsActive = request.IsActive
    };

    public void Update(FoodCategory foodCategory, UpdateFoodCategoryRequest request, DateTime utcNow)
    {
        if (request.Name is not null)
            foodCategory.Name = Required(request.Name, nameof(request.Name));

        if (request.Description is not null)
            foodCategory.Description = Optional(request.Description);

        if (request.Code is not null)
            foodCategory.Code = Code(request.Code, nameof(request.Code));

        if (request.SortOrder.HasValue)
            foodCategory.SortOrder = request.SortOrder.Value;

        if (request.IconUrl is not null)
            foodCategory.IconUrl = Optional(request.IconUrl);

        if (request.Color is not null)
            foodCategory.Color = Optional(request.Color);

        if (request.IsActive.HasValue)
            foodCategory.IsActive = request.IsActive.Value;

        foodCategory.DateModif = utcNow;
    }
}
