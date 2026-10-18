using Foodiya.Application.DTOs.FoodCategory.Request;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Factories;

public interface IFoodCategoryFactory
{
    FoodCategory Create(CreateFoodCategoryRequest request);
    void Update(FoodCategory foodCategory, UpdateFoodCategoryRequest request, DateTime utcNow);
}
