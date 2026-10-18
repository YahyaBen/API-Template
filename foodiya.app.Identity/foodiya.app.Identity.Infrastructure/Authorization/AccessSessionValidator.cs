using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using foodiya.app.Identity.Domain.Enums;
using foodiya.app.Identity.Infrastructure.Persistence;

namespace foodiya.app.Identity.Infrastructure.Authorization;

internal sealed class AccessSessionValidator(
    IdentityDataContext dataContext,
    TimeProvider timeProvider) : IAccessSessionValidator
{
    public async Task<ValidatedAccessSession?> ValidateAsync(
        ClaimsPrincipal principal,
        CancellationToken cancellationToken)
    {
        if (!TryReadGuid(principal, JwtRegisteredClaimNames.Sub, out var userId)
            || !TryReadGuid(principal, "sid", out var sessionId)
            || !TryReadGuid(principal, "did", out var deviceId))
        {
            return null;
        }

        var securityStamp = principal.FindFirst("sst")?.Value;
        if (string.IsNullOrWhiteSpace(securityStamp))
        {
            return null;
        }

        var user = await dataContext.Users
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == userId, cancellationToken);
        if (user is null
            || !user.EmailConfirmed
            || user.Status != AccountStatus.Active
            || !string.Equals(user.SecurityStamp, securityStamp, StringComparison.Ordinal))
        {
            return null;
        }

        var now = timeProvider.GetUtcNow();
        var session = await dataContext.UserSessions
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == sessionId, cancellationToken);
        if (session is null
            || session.UserId != userId
            || session.DeviceId != deviceId
            || !session.IsActive(now))
        {
            return null;
        }

        var device = await dataContext.UserDevices
            .AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == deviceId, cancellationToken);
        if (device is null
            || device.UserId != userId
            || device.RevokedAtUtc is not null)
        {
            return null;
        }

        return new ValidatedAccessSession(user, session, device);
    }

    private static bool TryReadGuid(
        ClaimsPrincipal principal,
        string claimType,
        out Guid value) => Guid.TryParse(principal.FindFirst(claimType)?.Value, out value);
}
