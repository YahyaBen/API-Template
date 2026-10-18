using System;
using System.Collections.Generic;

namespace Foodiya.Domain.Models;

public partial class RecipeIngredient
{
    public int RecipeId { get; set; }

    public int IngredientId { get; set; }

    public decimal Quantity { get; set; }

    public int? UnitId { get; set; }

    public bool IsOptional { get; set; }

    public string? Notes { get; set; }

    public int SortOrder { get; set; }

    public Guid Uuid { get; set; }

    public string Code { get; set; } = null!;

    public virtual Ingredient Ingredient { get; set; } = null!;

    public virtual Recipe Recipe { get; set; } = null!;

    public virtual Unit? Unit { get; set; }
}
