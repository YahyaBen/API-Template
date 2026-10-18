using System;
using System.Collections.Generic;

namespace Foodiya.Domain.Models;

public partial class MoroccanCity
{
    public int Id { get; set; }

    public int RegionId { get; set; }

    public string Name { get; set; } = null!;

    public string Slug { get; set; } = null!;

    public bool IsActive { get; set; }

    public int SortOrder { get; set; }

    public DateTime DateInsert { get; set; }

    public DateTime? DateModif { get; set; }

    public Guid Uuid { get; set; }

    public string Code { get; set; } = null!;

    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();

    public virtual MoroccanRegion Region { get; set; } = null!;
}
