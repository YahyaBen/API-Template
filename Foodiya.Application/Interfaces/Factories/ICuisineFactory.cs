using Foodiya.Application.DTOs.Cuisine.Request;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Factories;

public interface ICuisineFactory
{
    Cuisine Create(CreateCuisineRequest request);
    void Update(Cuisine cuisine, UpdateCuisineRequest request, DateTime utcNow);
}
