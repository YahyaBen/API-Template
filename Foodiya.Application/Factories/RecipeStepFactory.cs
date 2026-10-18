using Foodiya.Application.DTOs.RecipeStep.Request;
using Foodiya.Application.Interfaces.Factories;
using Foodiya.Domain.Exceptions;
using Foodiya.Domain.Extensions;
using Foodiya.Domain.Models;
using static Foodiya.Application.Factories.Helpers.EntityNormalizationHelper;

namespace Foodiya.Application.Factories;

public sealed class RecipeStepFactory : IRecipeStepFactory
{
    public RecipeStep Create(CreateRecipeStepItemRequest request) => new()
    {
        RecipeId = request.RecipeId,
        StepNumber = request.StepNumber,
        Title = Optional(request.Title),
        Instruction = Required(request.Instruction, nameof(request.Instruction)),
        DurationMinutes = request.DurationMinutes,
        Code = EntityCodeGenerator.For("RST")
    };

    public void Update(RecipeStep recipeStep, UpdateRecipeStepItemRequest request)
    {
        if (request.StepNumber.HasValue)
            recipeStep.StepNumber = request.StepNumber.Value;

        if (request.Title is not null)
            recipeStep.Title = Optional(request.Title);

        if (request.Instruction is not null)
            recipeStep.Instruction = Required(request.Instruction, nameof(request.Instruction));

        if (request.ClearDurationMinutes)
            recipeStep.DurationMinutes = null;
        else if (request.DurationMinutes.HasValue)
            recipeStep.DurationMinutes = request.DurationMinutes.Value;
    }
}
