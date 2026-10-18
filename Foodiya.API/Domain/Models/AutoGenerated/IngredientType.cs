using System;
using System.Collections.Generic;

namespace Foodiya.Domain.Models;

public partial class IngredientType
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;

    public bool IsActive { get; set; }

    public int SortOrder { get; set; }

    public string? IconUrl { get; set; }

    public string? Color { get; set; }

    public DateTime DateInsert { get; set; }

    public DateTime? DateModif { get; set; }

    public Guid Uuid { get; set; }

    public virtual ICollection<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
}
