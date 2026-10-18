using System.ComponentModel.DataAnnotations;

namespace foodiya.app.Identity.Contracts.Accounts;

/// <summary>Requests password recovery instructions by email.</summary>
public sealed record ForgotPasswordRequest
{
    [Required, EmailAddress, MaxLength(256)]
    public required string Email { get; init; }
}
