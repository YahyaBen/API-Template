namespace foodiya.app.Identity.Contracts.Authentication;

/// <summary>Authenticated identity and its rotating token pair.</summary>
public sealed record AuthenticationTokenResponse(
    Guid UserId,
    string Email,
    string FirstName,
    string LastName,
    IReadOnlyCollection<string> Roles,
    string AccessToken,
    string RefreshToken,
    string TokenType,
    DateTimeOffset AccessTokenExpiresAtUtc,
    DateTimeOffset RefreshTokenExpiresAtUtc,
    Guid SessionId,
    Guid DeviceId);
