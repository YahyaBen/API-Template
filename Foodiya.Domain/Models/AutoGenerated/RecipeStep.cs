using System;
using System.Collections.Generic;

namespace Foodiya.Domain.Models;

public partial class RecipeStep
{
    public int Id { get; set; }

    public int RecipeId { get; set; }

    public int StepNumber { get; set; }

    public string? Title { get; set; }

    public string Instruction { get; set; } = null!;

    public int? DurationMinutes { get; set; }

    public Guid Uuid { get; set; }

    public string Code { get; set; } = null!;

    public virtual Recipe Recipe { get; set; } = null!;
}
