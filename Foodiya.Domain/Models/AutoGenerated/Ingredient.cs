using System;
using System.Collections.Generic;

namespace Foodiya.Domain.Models;

public partial class Ingredient
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public int IngredientTypeId { get; set; }

    public int? DefaultUnitId { get; set; }

    public decimal? CaloriesPer100g { get; set; }

    public bool IsActive { get; set; }

    public Guid Uuid { get; set; }

    public string Code { get; set; } = null!;

    public virtual Unit? DefaultUnit { get; set; }

    public virtual ICollection<IngredientImage> IngredientImages { get; set; } = new List<IngredientImage>();

    public virtual IngredientType IngredientType { get; set; } = null!;

    public virtual ICollection<RecipeIngredient> RecipeIngredients { get; set; } = new List<RecipeIngredient>();
}
