using System;
using System.Collections.Generic;

namespace Foodiya.Domain.Models;

public partial class RecipeNutrition
{
    public int RecipeId { get; set; }

    public decimal CaloriesPerServing { get; set; }

    public decimal? ProteinGrams { get; set; }

    public decimal? CarbsGrams { get; set; }

    public decimal? FatGrams { get; set; }

    public string? Color { get; set; }

    public Guid Uuid { get; set; }

    public virtual Recipe Recipe { get; set; } = null!;
}
