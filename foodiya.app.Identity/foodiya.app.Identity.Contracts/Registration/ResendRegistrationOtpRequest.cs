using System.ComponentModel.DataAnnotations;

namespace foodiya.app.Identity.Contracts.Registration;

/// <summary>Requests a replacement registration confirmation code.</summary>
public sealed record ResendRegistrationOtpRequest
{
    /// <summary>Email address belonging to the pending account.</summary>
    [Required, EmailAddress, StringLength(256)]
    public required string Email { get; init; }
}
