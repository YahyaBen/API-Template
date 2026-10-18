using System;
using System.Collections.Generic;

namespace Foodiya.Domain.Models;

public partial class RecipeLike
{
    public int RecipeId { get; set; }

    public int UserId { get; set; }

    public DateTime LikedAt { get; set; }

    public virtual Recipe Recipe { get; set; } = null!;

    public virtual AppUser User { get; set; } = null!;
}
