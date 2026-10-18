using Foodiya.Application.DTOs.Difficulty.Request;
using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Factories;

public interface IDifficultyFactory
{
    Difficulty Create(CreateDifficultyRequest request);
    void Update(Difficulty difficulty, UpdateDifficultyRequest request, DateTime utcNow);
}
