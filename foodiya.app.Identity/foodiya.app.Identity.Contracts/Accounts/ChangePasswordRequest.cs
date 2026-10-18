using System.ComponentModel.DataAnnotations;

namespace foodiya.app.Identity.Contracts.Accounts;

/// <summary>Changes an authenticated account's password using its current password.</summary>
public sealed record ChangePasswordRequest
{
    [Required, MaxLength(128)]
    public required string CurrentPassword { get; init; }

    [Required, MinLength(8), MaxLength(128)]
    public required string NewPassword { get; init; }
}
