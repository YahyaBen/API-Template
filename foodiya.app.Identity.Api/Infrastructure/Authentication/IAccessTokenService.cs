using foodiya.app.Identity.Infrastructure.Identity;

namespace foodiya.app.Identity.Infrastructure.Authentication;

internal interface IAccessTokenService
{
    (string Token, DateTimeOffset ExpiresAtUtc) Create(
        ApplicationUser user,
        IReadOnlyCollection<string> roles,
        Guid sessionId,
        Guid deviceId,
        DateTimeOffset sessionExpiresAtUtc);
}
