namespace Foodiya.Application.DTOs.Recipe.Response;

public sealed class RecipeStepResponse
{
    public int StepNumber { get; set; }
    public string? Title { get; set; }
    public string Instruction { get; set; } = string.Empty;
    public int? DurationMinutes { get; set; }
}
