using System.ComponentModel.DataAnnotations;

namespace foodiya.app.Identity.Contracts.Authentication;

/// <summary>Rotates an active refresh token and returns a new token pair.</summary>
public sealed record RefreshAccessTokenRequest
{
    /// <summary>Opaque refresh token returned by login or a previous refresh.</summary>
    [Required, StringLength(512, MinimumLength = 32)]
    public required string RefreshToken { get; init; }
}
