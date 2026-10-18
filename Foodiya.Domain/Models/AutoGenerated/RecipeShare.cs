using System;
using System.Collections.Generic;

namespace Foodiya.Domain.Models;

public partial class RecipeShare
{
    public int Id { get; set; }

    public int RecipeId { get; set; }

    public int SharedByUserId { get; set; }

    public int? SharedWithUserId { get; set; }

    public string ShareChannel { get; set; } = null!;

    public string? ShareMessage { get; set; }

    public DateTime SharedAt { get; set; }

    public Guid Uuid { get; set; }

    public string Code { get; set; } = null!;

    public virtual Recipe Recipe { get; set; } = null!;

    public virtual AppUser SharedByUser { get; set; } = null!;

    public virtual AppUser? SharedWithUser { get; set; }
}
