using System;
using System.Collections.Generic;

namespace Foodiya.Domain.Models;

public partial class PresetAvatarImage
{
    public int Id { get; set; }

    public string Code { get; set; } = null!;

    public string Label { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;

    public string? BackgroundColor { get; set; }

    public int SortOrder { get; set; }

    public bool IsActive { get; set; }

    public Guid Uuid { get; set; }

    public virtual ICollection<AppUser> AppUsers { get; set; } = new List<AppUser>();
}
