using System;
using System.Collections.Generic;

namespace Foodiya.Domain.Models;

public partial class AppUserRefreshToken
{
    public int Id { get; set; }

    public int AppUserId { get; set; }

    public string TokenHash { get; set; } = null!;

    public DateTime ExpiresAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? RevokedAtUtc { get; set; }

    public string? ReplacedByTokenHash { get; set; }

    public Guid Uuid { get; set; }

    public string Code { get; set; } = null!;

    public virtual AppUser AppUser { get; set; } = null!;
}
