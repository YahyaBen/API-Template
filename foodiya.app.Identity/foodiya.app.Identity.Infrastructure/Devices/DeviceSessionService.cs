using System.Data;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using foodiya.app.Identity.Application.Exceptions;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Contracts.Accounts;
using foodiya.app.Identity.Contracts.Devices;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Domain.Enums;
using foodiya.app.Identity.Infrastructure.Authorization;
using foodiya.app.Identity.Infrastructure.Options;
using foodiya.app.Identity.Infrastructure.Persistence;

namespace foodiya.app.Identity.Infrastructure.Devices;

internal sealed class DeviceSessionService(
    IdentityDataContext db,
    IAccessSessionValidator accessValidator,
    IOtpCodeService otpCodes,
    IIdentityEmailSender emailSender,
    IOptions<RegistrationOptions> options,
    TimeProvider clock) : IDeviceSessionService
{
    public async Task<DeviceListResponse> GetDevicesAsync(ClaimsPrincipal principal, int skip, int take, CancellationToken cancellationToken)
    {
        ValidatePage(skip, take);
        var access = await GetAccessAsync(principal, cancellationToken);
        var devices = await db.UserDevices.AsNoTracking().Where(device => device.UserId == access.User.Id)
            .OrderByDescending(device => device.LastSeenAtUtc).ThenBy(device => device.Id)
            .Skip(skip).Take(take)
            .Select(device => new DeviceResponse(device.Id, device.Platform, device.DeviceName,
                device.FirstSeenAtUtc, device.LastSeenAtUtc, device.VerifiedAtUtc, device.RevokedAtUtc,
                device.Id == access.Device.Id, device.FirebaseInstallationId != null))
            .ToListAsync(cancellationToken);
        return new DeviceListResponse(devices, skip, take);
    }

    public async Task<SessionListResponse> GetSessionsAsync(ClaimsPrincipal principal, int skip, int take, CancellationToken cancellationToken)
    {
        ValidatePage(skip, take);
        var access = await GetAccessAsync(principal, cancellationToken);
        var sessions = await db.UserSessions.AsNoTracking().Where(session => session.UserId == access.User.Id)
            .OrderByDescending(session => session.CreatedAtUtc).ThenBy(session => session.Id)
            .Skip(skip).Take(take).ToListAsync(cancellationToken);
        var now = clock.GetUtcNow();
        return new SessionListResponse(sessions.Select(session => new SessionResponse(session.Id,
            session.DeviceId, session.CreatedAtUtc, session.ExpiresAtUtc, session.RevokedAtUtc,
            session.IsActive(now), session.Id == access.Session.Id)).ToArray(), skip, take);
    }

    public async Task RevokeDeviceAsync(ClaimsPrincipal principal, Guid deviceId, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var access = await GetAccessAsync(principal, cancellationToken);
        var device = await db.UserDevices.SingleOrDefaultAsync(
            candidate => candidate.Id == deviceId && candidate.UserId == access.User.Id, cancellationToken)
            ?? throw NotFound();
        if (device.RevokedAtUtc is null)
        {
            var now = clock.GetUtcNow();
            device.RevokedAtUtc = now;
            device.RevocationReason = "DeviceRevoked";
            device.FirebaseInstallationId = null;
            device.FirebaseInstallationUpdatedAtUtc = now;
            var sessions = db.UserSessions.Where(session => session.DeviceId == deviceId && session.UserId == access.User.Id);
            await RevokeAsync(sessions, "DeviceRevoked", cancellationToken);
            await db.OtpChallenges.Where(otp => otp.UserId == access.User.Id && otp.Purpose == OtpPurpose.DeviceLogin
                && otp.DeviceInstallationIdHash == device.InstallationIdHash && otp.InvalidatedAtUtc == null)
                .ExecuteUpdateAsync(updates => updates.SetProperty(otp => otp.InvalidatedAtUtc, now), cancellationToken);
            AddEvent("UserDeviceRevoked", access.User.Id, deviceId);
            await db.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task RevokeSessionAsync(ClaimsPrincipal principal, Guid sessionId, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var access = await GetAccessAsync(principal, cancellationToken);
        var session = await db.UserSessions.AsNoTracking().SingleOrDefaultAsync(
            candidate => candidate.Id == sessionId && candidate.UserId == access.User.Id, cancellationToken)
            ?? throw NotFound();
        if (session.RevokedAtUtc is null)
        {
            await RevokeAsync(db.UserSessions.Where(candidate => candidate.Id == sessionId), "SessionRevoked", cancellationToken);
            AddEvent("UserSessionRevoked", access.User.Id, session.DeviceId, sessionId);
            await db.SaveChangesAsync(cancellationToken);
        }

        await transaction.CommitAsync(cancellationToken);
    }

    public async Task SetFirebaseInstallationAsync(ClaimsPrincipal principal, string? firebaseInstallationId, CancellationToken cancellationToken)
    {
        if (firebaseInstallationId is not null
            && (string.IsNullOrWhiteSpace(firebaseInstallationId) || firebaseInstallationId.Trim().Length > 128))
        {
            throw new IdentityValidationException(
                "FIREBASE_INSTALLATION_INVALID",
                "A Firebase installation ID must contain 1 to 128 characters.");
        }

        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var access = await GetAccessAsync(principal, cancellationToken);
        var value = firebaseInstallationId?.Trim();
        var now = clock.GetUtcNow();
        // An FID may migrate between this account's devices, but never mutate another account.
        if (value is not null)
        {
            if (await db.UserDevices.AnyAsync(
                device => device.UserId != access.User.Id && device.FirebaseInstallationId == value,
                cancellationToken))
            {
                throw new IdentityConflictException(
                    "FIREBASE_INSTALLATION_UNAVAILABLE",
                    "This Firebase installation cannot be assigned.");
            }

            await db.UserDevices.Where(device => device.UserId == access.User.Id && device.Id != access.Device.Id
                && device.FirebaseInstallationId == value)
                .ExecuteUpdateAsync(updates => updates
                    .SetProperty(device => device.FirebaseInstallationId, (string?)null)
                    .SetProperty(device => device.FirebaseInstallationUpdatedAtUtc, now), cancellationToken);
        }

        await db.UserDevices.Where(device => device.Id == access.Device.Id)
            .ExecuteUpdateAsync(updates => updates
                .SetProperty(device => device.FirebaseInstallationId, value)
                .SetProperty(device => device.FirebaseInstallationUpdatedAtUtc, now), cancellationToken);
        // Firebase installation IDs never enter event payloads or API responses.
        AddEvent(value is null ? "FirebaseInstallationCleared" : "FirebaseInstallationUpdated",
            access.User.Id, access.Device.Id);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<AccountChallengeResponse> RequestVerificationAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var access = await GetAccessAsync(principal, cancellationToken);
        if (access.Device.VerifiedAtUtc is not null)
        {
            throw new IdentityConflictException("DEVICE_ALREADY_VERIFIED", "The current device is already verified.");
        }

        var now = clock.GetUtcNow();
        var challenges = db.OtpChallenges.Where(otp => otp.UserId == access.User.Id && otp.Purpose == OtpPurpose.DeviceLogin
            && otp.DeviceInstallationIdHash == access.Device.InstallationIdHash);
        var latest = await challenges.OrderByDescending(otp => otp.CreatedAtUtc).FirstOrDefaultAsync(cancellationToken);
        if (latest is not null && latest.CreatedAtUtc.AddSeconds(options.Value.OtpResendCooldownSeconds) > now)
        {
            throw new IdentityThrottleException("OTP_RESEND_TOO_SOON", "Wait before requesting another device code.",
                latest.CreatedAtUtc.AddSeconds(options.Value.OtpResendCooldownSeconds));
        }

        await challenges.Where(otp => otp.InvalidatedAtUtc == null && otp.ConsumedAtUtc == null)
            .ExecuteUpdateAsync(updates => updates.SetProperty(otp => otp.InvalidatedAtUtc, now), cancellationToken);
        var challenge = new OtpChallenge
        {
            UserId = access.User.Id,
            NormalizedEmail = access.User.NormalizedEmail!,
            Purpose = OtpPurpose.DeviceLogin,
            DeviceInstallationIdHash = access.Device.InstallationIdHash,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddMinutes(options.Value.OtpLifetimeMinutes),
            MaxAttempts = options.Value.OtpMaxAttempts,
            CodeHash = string.Empty
        };
        var code = otpCodes.GenerateCode();
        challenge.CodeHash = otpCodes.HashCode(challenge.Id, code);
        db.OtpChallenges.Add(challenge);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        await emailSender.SendAccountOtpAsync(
            access.User.Email!,
            new IdentityEmailContext(
                access.User.FirstName,
                access.User.PreferredLanguage,
                access.Device.DeviceName,
                access.Device.AppVersion,
                access.Device.LastSeenAtUtc),
            challenge.Purpose,
            challenge.Id,
            code,
            challenge.ExpiresAtUtc,
            cancellationToken);
        return new AccountChallengeResponse(challenge.Id, challenge.ExpiresAtUtc);
    }

    public async Task ConfirmVerificationAsync(ClaimsPrincipal principal, ConfirmDeviceVerificationRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);
        var access = await GetAccessAsync(principal, cancellationToken);
        var challenge = await db.OtpChallenges.SingleOrDefaultAsync(otp => otp.Id == request.ChallengeId
            && otp.UserId == access.User.Id && otp.Purpose == OtpPurpose.DeviceLogin
            && otp.DeviceInstallationIdHash == access.Device.InstallationIdHash, cancellationToken);
        var now = clock.GetUtcNow();
        if (challenge is null || !challenge.IsUsable(now) || challenge.NormalizedEmail != access.User.NormalizedEmail)
        {
            throw InvalidCode();
        }

        if (!otpCodes.VerifyCode(challenge.Id, request.Code, challenge.CodeHash))
        {
            challenge.FailedAttempts++;
            if (challenge.FailedAttempts >= challenge.MaxAttempts)
            {
                challenge.InvalidatedAtUtc = now;
            }

            await db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            throw InvalidCode();
        }

        challenge.ConsumedAtUtc = now;
        await db.UserDevices.Where(device => device.Id == access.Device.Id)
            .ExecuteUpdateAsync(updates => updates.SetProperty(device => device.VerifiedAtUtc, now), cancellationToken);
        AddEvent("UserDeviceVerified", access.User.Id, access.Device.Id);
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private async Task RevokeAsync(IQueryable<UserSession> sessions, string reason, CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow();
        var ids = sessions.Select(session => session.Id);
        await db.RefreshTokens.Where(token => ids.Contains(token.SessionId) && token.RevokedAtUtc == null)
            .ExecuteUpdateAsync(updates => updates.SetProperty(token => token.RevokedAtUtc, now)
                .SetProperty(token => token.RevocationReason, reason), cancellationToken);
        await sessions.Where(session => session.RevokedAtUtc == null)
            .ExecuteUpdateAsync(updates => updates.SetProperty(session => session.RevokedAtUtc, now)
                .SetProperty(session => session.RevocationReason, reason), cancellationToken);
    }

    private async Task<ValidatedAccessSession> GetAccessAsync(ClaimsPrincipal principal, CancellationToken cancellationToken) =>
        await accessValidator.ValidateAsync(principal, cancellationToken)
        ?? throw new IdentityUnauthorizedException("ACCESS_SESSION_INVALID", "The authenticated session is no longer valid.");

    private void AddEvent(string type, Guid userId, Guid deviceId, Guid? sessionId = null) => db.OutboxMessages.Add(new OutboxMessage
    {
        Type = type,
        OccurredAtUtc = clock.GetUtcNow(),
        Payload = JsonSerializer.Serialize(new { UserId = userId, DeviceId = deviceId, SessionId = sessionId })
    });

    private static void ValidatePage(int skip, int take)
    {
        if (skip < 0 || take is < 1 or > 100)
        {
            throw new IdentityValidationException("PAGINATION_INVALID", "Skip must be non-negative and take must be between 1 and 100.");
        }
    }

    private static IdentityNotFoundException NotFound() => new("DEVICE_OR_SESSION_NOT_FOUND", "The requested device or session was not found.");
    private static IdentityValidationException InvalidCode() => new("OTP_INVALID_OR_EXPIRED", "The confirmation code is invalid or expired.");
}
