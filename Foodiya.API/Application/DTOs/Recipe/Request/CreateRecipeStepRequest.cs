using System.ComponentModel.DataAnnotations;

namespace Foodiya.Application.DTOs.Recipe.Request;

public sealed class CreateRecipeStepRequest
{
    [Range(1, 100)]
    public int StepNumber { get; set; }

    [StringLength(150)]
    public string? Title { get; set; }

    [Required, StringLength(2000)]
    public string Instruction { get; set; } = string.Empty;

    public int? DurationMinutes { get; set; }
}
