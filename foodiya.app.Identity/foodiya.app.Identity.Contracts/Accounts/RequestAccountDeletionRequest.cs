using System.ComponentModel.DataAnnotations;

namespace foodiya.app.Identity.Contracts.Accounts;

/// <summary>Reauthenticates before emailing an account deletion challenge.</summary>
public sealed record RequestAccountDeletionRequest
{
    [Required, MaxLength(128)]
    public required string CurrentPassword { get; init; }
}
