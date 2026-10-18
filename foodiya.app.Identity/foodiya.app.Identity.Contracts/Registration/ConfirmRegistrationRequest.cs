using System.ComponentModel.DataAnnotations;

namespace foodiya.app.Identity.Contracts.Registration;

/// <summary>Confirmation challenge and one-time code.</summary>
public sealed record ConfirmRegistrationRequest
{
    /// <summary>Identifier returned by the registration or resend endpoint.</summary>
    public required Guid ChallengeId { get; init; }

    /// <summary>Six-digit email confirmation code.</summary>
    [Required, RegularExpression("^[0-9]{6}$")]
    public required string Code { get; init; }
}
