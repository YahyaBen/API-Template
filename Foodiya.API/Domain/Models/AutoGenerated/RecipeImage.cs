using System;
using System.Collections.Generic;

namespace Foodiya.Domain.Models;

public partial class RecipeImage
{
    public int Id { get; set; }

    public int RecipeId { get; set; }

    public string ImageUrl { get; set; } = null!;

    public string? AltText { get; set; }

    public bool IsPrimary { get; set; }

    public int SortOrder { get; set; }

    public Guid Uuid { get; set; }

    public string Code { get; set; } = null!;

    public virtual Recipe Recipe { get; set; } = null!;
}
