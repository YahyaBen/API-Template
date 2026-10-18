using System;
using System.Collections.Generic;

namespace Foodiya.Domain.Models;

public partial class ChefProfile
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public string DisplayName { get; set; } = null!;

    public string? Bio { get; set; }

    public string? Specialty { get; set; }

    public int? YearsOfExperience { get; set; }

    public bool IsVerified { get; set; }

    public DateTime DateInsert { get; set; }

    public bool IsActive { get; set; }

    public DateTime? DateModif { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedByUserId { get; set; }

    public string DeleteReason { get; set; } = null!;

    public string? Color { get; set; }

    public Guid Uuid { get; set; }

    public string Code { get; set; } = null!;

    public virtual AppUser? DeletedByUser { get; set; }

    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();

    public virtual AppUser User { get; set; } = null!;
}
