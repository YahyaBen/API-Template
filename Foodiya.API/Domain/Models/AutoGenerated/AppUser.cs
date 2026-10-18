using System;
using System.Collections.Generic;

namespace Foodiya.Domain.Models;

public partial class AppUser
{
    public int Id { get; set; }

    public string UserName { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Email { get; set; } = null!;

    public byte[] PasswordHash { get; set; } = null!;

    public byte[] PasswordSalt { get; set; } = null!;

    public string? ProfileImageUrl { get; set; }

    public int? PresetAvatarImageId { get; set; }

    public DateTime DateInsert { get; set; }

    public DateTime? DateModif { get; set; }

    public bool IsActive { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int? DeletedByUserId { get; set; }

    public string DeleteReason { get; set; } = null!;

    public string Role { get; set; } = null!;

    public string? Color { get; set; }

    public Guid Uuid { get; set; }

    public string Code { get; set; } = null!;

    public virtual ICollection<AppUserRefreshToken> AppUserRefreshTokens { get; set; } = new List<AppUserRefreshToken>();

    public virtual ICollection<ChefProfile> ChefProfileDeletedByUsers { get; set; } = new List<ChefProfile>();

    public virtual ChefProfile? ChefProfileUser { get; set; }

    public virtual AppUser? DeletedByUser { get; set; }

    public virtual ICollection<AppUser> InverseDeletedByUser { get; set; } = new List<AppUser>();

    public virtual PresetAvatarImage? PresetAvatarImage { get; set; }

    public virtual ICollection<RecipeLike> RecipeLikes { get; set; } = new List<RecipeLike>();

    public virtual ICollection<RecipeShare> RecipeShareSharedByUsers { get; set; } = new List<RecipeShare>();

    public virtual ICollection<RecipeShare> RecipeShareSharedWithUsers { get; set; } = new List<RecipeShare>();

    public virtual ICollection<Recipe> Recipes { get; set; } = new List<Recipe>();
}
