using Foodiya.Domain.Models;

namespace Foodiya.Application.Interfaces.Auth;

/// <summary>
/// Generates, validates, and rotates opaque refresh tokens.
/// </summary>
public interface IRefreshTokenService
{
    /// <summary>
    /// Creates a new refresh token for the user and persists its hash.
    /// Returns the raw (unhashed) token to send to the client.
    /// </summary>
    Task<string> CreateRefreshTokenAsync(int appUserId, CancellationToken ct = default);

    /// <summary>
    /// Validates a raw refresh token, revokes it, and issues a rotated replacement.
    /// Returns the user entity and the new raw refresh token.
    /// Throws if the token is invalid, expired, or already revoked.
    /// </summary>
    Task<(AppUser user, string newRefreshToken)> RotateRefreshTokenAsync(string rawRefreshToken, int appUserId, CancellationToken ct = default);

    /// <summary>
    /// Revokes all active refresh tokens for a user (e.g. on password change / logout-all).
    /// </summary>
    Task RevokeAllForUserAsync(int appUserId, CancellationToken ct = default);
}
