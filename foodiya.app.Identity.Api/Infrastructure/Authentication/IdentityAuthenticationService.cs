using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using foodiya.app.Identity.Application.Exceptions;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Contracts.Authentication;
using foodiya.app.Identity.Domain.Constants;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Domain.Enums;
using foodiya.app.Identity.Infrastructure.Identity;
using foodiya.app.Identity.Infrastructure.Options;
using foodiya.app.Identity.Infrastructure.Persistence;

namespace foodiya.app.Identity.Infrastructure.Authentication;

internal sealed class IdentityAuthenticationService(
    UserManager<ApplicationUser> userManager,
    IdentityDataContext dataContext,
    IAccessTokenService accessTokenService,
    ITokenCryptographyService tokenCryptography,
    ILoginTimingProtection loginTimingProtection,
    IGoogleIdTokenValidator googleIdTokenValidator,
    IOptions<AuthenticationTokenOptions> options,
    TimeProvider timeProvider) : IIdentityAuthenticationService
{
    private readonly GoogleAccountLinker _googleAccounts = new(userManager, dataContext, timeProvider);
    private readonly AuthenticationTokenOptions _options = options.Value;

    public async Task<AuthenticationTokenResponse> LoginAsync(
        LoginRequest request,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var identifier = request.Login.Trim();
        var userByEmail = await userManager.FindByEmailAsync(identifier);
        var userByName = await userManager.FindByNameAsync(identifier);
        var user = userByEmail ?? userByName;

        if (user is null)
        {
            loginTimingProtection.VerifyDummyPassword(request.Password);
            throw InvalidCredentials();
        }

        var now = timeProvider.GetUtcNow();
        if (await userManager.IsLockedOutAsync(user))
        {
            throw AccountLocked(user, now);
        }

        if (!await userManager.CheckPasswordAsync(user, request.Password))
        {
            EnsureSucceeded(await userManager.AccessFailedAsync(user));
            if (await userManager.IsLockedOutAsync(user))
            {
                throw AccountLocked(user, now);
            }

            throw InvalidCredentials();
        }

        if (user.AccessFailedCount > 0)
        {
            EnsureSucceeded(await userManager.ResetAccessFailedCountAsync(user));
        }

        IdentityAccountRules.EnsureCanAuthenticate(user);

        return await CreateSessionAsync(
            user,
            new DeviceMetadata(
                request.DeviceInstallationId,
                request.Platform,
                request.DeviceName,
                request.OsVersion,
                request.AppVersion),
            ipAddress,
            cancellationToken);
    }

    public async Task<AuthenticationTokenResponse> SignInWithGoogleAsync(
        GoogleSignInRequest request,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var identity = await googleIdTokenValidator.ValidateAsync(
            request.IdToken,
            cancellationToken);
        if (!identity.EmailVerified)
        {
            throw new IdentityUnauthorizedException(
                "GOOGLE_EMAIL_NOT_VERIFIED",
                "The Google account email address is not verified.");
        }

        var user = await userManager.FindByLoginAsync(GoogleAccountLinker.GoogleLoginProvider, identity.Subject)
            ?? await _googleAccounts.ResolveAsync(identity, cancellationToken);

        IdentityAccountRules.EnsureCanAuthenticate(user);
        var now = timeProvider.GetUtcNow();
        if (await userManager.IsLockedOutAsync(user))
        {
            throw AccountLocked(user, now);
        }

        return await CreateSessionAsync(
            user,
            new DeviceMetadata(
                request.DeviceInstallationId,
                request.Platform,
                request.DeviceName,
                request.OsVersion,
                request.AppVersion),
            ipAddress,
            cancellationToken);
    }

    private async Task<AuthenticationTokenResponse> CreateSessionAsync(
        ApplicationUser user,
        DeviceMetadata metadata,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var deviceHash = tokenCryptography.HashDeviceInstallationId(
            metadata.DeviceInstallationId);

        await using var transaction = await dataContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        // A credential change may commit between the password check and session creation.
        // Hold the current user row through commit and reject a stale authentication result.
        var currentUser = await dataContext.Users.AsNoTracking()
            .SingleOrDefaultAsync(candidate => candidate.Id == user.Id, cancellationToken);
        if (currentUser is null || currentUser.SecurityStamp != user.SecurityStamp
            || !IdentityAccountRules.CanAuthenticate(currentUser))
        {
            throw InvalidCredentials();
        }

        var roles = (await userManager.GetRolesAsync(user))
            .Order(StringComparer.Ordinal)
            .ToArray();

        var device = await dataContext.UserDevices.SingleOrDefaultAsync(
            candidate => candidate.UserId == user.Id
                && candidate.InstallationIdHash == deviceHash,
            cancellationToken);

        if (device?.RevokedAtUtc is not null)
        {
            throw new IdentityForbiddenException(
                "DEVICE_REVOKED",
                "This device installation has been revoked.");
        }

        if (device is null)
        {
            device = new UserDevice
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                InstallationIdHash = deviceHash,
                FirstSeenAtUtc = now,
                LastSeenAtUtc = now
            };
            dataContext.UserDevices.Add(device);
        }

        UpdateDevice(device, metadata, ipAddress, now);

        var session = new UserSession
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            DeviceId = device.Id,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddDays(_options.RefreshTokenLifetimeDays),
            IpAddress = NormalizeOptional(ipAddress)
        };
        var refreshTokenValue = tokenCryptography.GenerateRefreshToken();
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SessionId = session.Id,
            TokenHash = tokenCryptography.HashRefreshToken(refreshTokenValue),
            CreatedAtUtc = now,
            ExpiresAtUtc = session.ExpiresAtUtc,
            CreatedByIp = NormalizeOptional(ipAddress)
        };

        dataContext.UserSessions.Add(session);
        dataContext.RefreshTokens.Add(refreshToken);
        await dataContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return CreateResponse(user, roles, session, refreshTokenValue);
    }

    public async Task<AuthenticationTokenResponse> RefreshAsync(
        RefreshAccessTokenRequest request,
        string? ipAddress,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var tokenHash = tokenCryptography.HashRefreshToken(request.RefreshToken);

        await using var transaction = await dataContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        // Serialize attempts for the same token before reading its state. Shared
        // SERIALIZABLE reads otherwise deadlock when concurrent refreshes upgrade locks.
        var currentToken = await dataContext.RefreshTokens
            .FromSqlInterpolated($"SELECT * FROM [dbo].[RefreshTokens] WITH (UPDLOCK) WHERE [TokenHash] = {tokenHash}")
            .AsNoTracking()
            .SingleOrDefaultAsync(cancellationToken);

        if (currentToken is null)
        {
            throw InvalidRefreshToken();
        }

        if (currentToken.RevokedAtUtc is not null)
        {
            await RevokeSessionAsync(
                currentToken.SessionId,
                now,
                NormalizeOptional(ipAddress),
                "RefreshTokenReuseDetected",
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            throw new IdentityUnauthorizedException(
                "REFRESH_TOKEN_REUSE_DETECTED",
                "Refresh token reuse was detected and the session was revoked.");
        }

        var session = await dataContext.UserSessions.SingleOrDefaultAsync(
            candidate => candidate.Id == currentToken.SessionId,
            cancellationToken);
        if (session is null || !session.IsActive(now))
        {
            throw InvalidRefreshToken();
        }

        if (!currentToken.IsActive(now))
        {
            await RevokeSessionAsync(
                currentToken.SessionId,
                now,
                NormalizeOptional(ipAddress),
                "Expired",
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            throw InvalidRefreshToken();
        }

        var user = await userManager.FindByIdAsync(currentToken.UserId.ToString())
            ?? throw InvalidRefreshToken();
        if (!IdentityAccountRules.CanAuthenticate(user))
        {
            await RevokeSessionAsync(
                session.Id,
                now,
                NormalizeOptional(ipAddress),
                "AccountNotActive",
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            IdentityAccountRules.EnsureCanAuthenticate(user);
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            await RevokeSessionAsync(
                session.Id,
                now,
                NormalizeOptional(ipAddress),
                "AccountLocked",
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            throw AccountLocked(user, now);
        }

        var device = await dataContext.UserDevices.SingleOrDefaultAsync(
            candidate => candidate.Id == session.DeviceId,
            cancellationToken);
        if (device is null || device.RevokedAtUtc is not null)
        {
            await RevokeSessionAsync(
                session.Id,
                now,
                NormalizeOptional(ipAddress),
                "DeviceRevoked",
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            throw new IdentityForbiddenException(
                "DEVICE_REVOKED",
                "This device installation has been revoked.");
        }

        var replacementValue = tokenCryptography.GenerateRefreshToken();
        var replacementHash = tokenCryptography.HashRefreshToken(replacementValue);
        var rotated = await dataContext.RefreshTokens
            .Where(token => token.Id == currentToken.Id
                && token.RevokedAtUtc == null
                && token.ExpiresAtUtc > now)
            .ExecuteUpdateAsync(
                updates => updates
                    .SetProperty(token => token.RevokedAtUtc, now)
                    .SetProperty(token => token.RevokedByIp, NormalizeOptional(ipAddress))
                    .SetProperty(token => token.RevocationReason, "Rotated")
                    .SetProperty(token => token.ReplacedByTokenHash, replacementHash),
                cancellationToken);

        if (rotated != 1)
        {
            await RevokeSessionAsync(
                session.Id,
                now,
                NormalizeOptional(ipAddress),
                "RefreshTokenReuseDetected",
                cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            throw new IdentityUnauthorizedException(
                "REFRESH_TOKEN_REUSE_DETECTED",
                "Refresh token reuse was detected and the session was revoked.");
        }

        dataContext.RefreshTokens.Add(new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            SessionId = session.Id,
            TokenHash = replacementHash,
            CreatedAtUtc = now,
            ExpiresAtUtc = session.ExpiresAtUtc,
            CreatedByIp = NormalizeOptional(ipAddress)
        });

        device.LastSeenAtUtc = now;
        device.LastIpAddress = NormalizeOptional(ipAddress);

        var roles = (await userManager.GetRolesAsync(user))
            .Order(StringComparer.Ordinal)
            .ToArray();
        await dataContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return CreateResponse(user, roles, session, replacementValue);
    }

    private AuthenticationTokenResponse CreateResponse(
        ApplicationUser user,
        IReadOnlyCollection<string> roles,
        UserSession session,
        string refreshToken)
    {
        var (accessToken, accessTokenExpiresAt) = accessTokenService.Create(
            user,
            roles,
            session.Id,
            session.DeviceId,
            session.ExpiresAtUtc);

        return new AuthenticationTokenResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.FirstName,
            user.LastName,
            roles,
            accessToken,
            refreshToken,
            "Bearer",
            accessTokenExpiresAt,
            session.ExpiresAtUtc,
            session.Id,
            session.DeviceId);
    }

    private async Task RevokeSessionAsync(
        Guid sessionId,
        DateTimeOffset now,
        string? ipAddress,
        string reason,
        CancellationToken cancellationToken)
    {
        await dataContext.UserSessions
            .Where(session => session.Id == sessionId && session.RevokedAtUtc == null)
            .ExecuteUpdateAsync(
                updates => updates
                    .SetProperty(session => session.RevokedAtUtc, now)
                    .SetProperty(session => session.RevocationReason, reason),
                cancellationToken);

        await dataContext.RefreshTokens
            .Where(token => token.SessionId == sessionId && token.RevokedAtUtc == null)
            .ExecuteUpdateAsync(
                updates => updates
                    .SetProperty(token => token.RevokedAtUtc, now)
                    .SetProperty(token => token.RevokedByIp, ipAddress)
                    .SetProperty(token => token.RevocationReason, reason),
                cancellationToken);
    }

    private static void UpdateDevice(
        UserDevice device,
        DeviceMetadata metadata,
        string? ipAddress,
        DateTimeOffset now)
    {
        device.Platform = NormalizeOptional(metadata.Platform);
        device.DeviceName = NormalizeOptional(metadata.DeviceName);
        device.OsVersion = NormalizeOptional(metadata.OsVersion);
        device.AppVersion = NormalizeOptional(metadata.AppVersion);
        device.LastIpAddress = NormalizeOptional(ipAddress);
        device.LastSeenAtUtc = now;
    }

    private static IdentityLockedException AccountLocked(
        ApplicationUser user,
        DateTimeOffset now) => new(
            "ACCOUNT_LOCKED",
            "The account is temporarily locked because of failed sign-in attempts.",
            user.LockoutEnd ?? now.AddMinutes(15));

    private static IdentityUnauthorizedException InvalidCredentials() => new(
        "INVALID_CREDENTIALS",
        "The email, user name, or password is incorrect.");

    private static IdentityUnauthorizedException InvalidRefreshToken() => new(
        "REFRESH_TOKEN_INVALID",
        "The refresh token is invalid or expired.");

    private static string? NormalizeOptional(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("ASP.NET Core Identity could not update login state.");
        }
    }

    private sealed record DeviceMetadata(
        string DeviceInstallationId,
        string? Platform,
        string? DeviceName,
        string? OsVersion,
        string? AppVersion);
}
