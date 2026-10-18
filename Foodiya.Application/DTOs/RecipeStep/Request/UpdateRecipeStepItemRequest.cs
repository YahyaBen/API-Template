using System.ComponentModel.DataAnnotations;

namespace Foodiya.Application.DTOs.RecipeStep.Request;

public sealed class UpdateRecipeStepItemRequest
{
    [Range(1, 100)]
    public int? StepNumber { get; set; }

    [StringLength(150)]
    public string? Title { get; set; }

    [StringLength(2000)]
    public string? Instruction { get; set; }

    [Range(0, 1440)]
    public int? DurationMinutes { get; set; }

    public bool ClearDurationMinutes { get; set; }
}
