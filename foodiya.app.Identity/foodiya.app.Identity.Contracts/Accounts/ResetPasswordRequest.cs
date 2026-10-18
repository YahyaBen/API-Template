using System.ComponentModel.DataAnnotations;

namespace foodiya.app.Identity.Contracts.Accounts;

/// <summary>Confirms a password recovery challenge and sets a new password.</summary>
public sealed record ResetPasswordRequest
{
    public required Guid ChallengeId { get; init; }

    [Required, RegularExpression("^[0-9]{6}$")]
    public required string Code { get; init; }

    [Required, MinLength(8), MaxLength(128)]
    public required string NewPassword { get; init; }
}
