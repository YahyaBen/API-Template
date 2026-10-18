using System.ComponentModel.DataAnnotations;

namespace foodiya.app.Identity.Contracts.Accounts;

/// <summary>Confirms deletion of the authenticated account with its email challenge.</summary>
public sealed record ConfirmAccountDeletionRequest
{
    public required Guid ChallengeId { get; init; }

    [Required, RegularExpression("^[0-9]{6}$")]
    public required string Code { get; init; }
}
