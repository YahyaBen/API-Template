using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using foodiya.app.Identity.Infrastructure.Options;
using foodiya.app.Identity.Infrastructure.Persistence;
using foodiya.app.Identity.Domain.Enums;

namespace foodiya.app.Identity.Infrastructure.Operations;

public sealed record CleanupResult(int Sessions, int Otps, int Events, int RegistrationAttempts, int DeletedAccounts);

internal sealed class IdentityCleanupService(
    IdentityDataContext db, IOptions<CleanupOptions> options, TimeProvider clock,
    ILogger<IdentityCleanupService> logger, IdentityTelemetry telemetry)
{
    public async Task<CleanupResult> RunAsync(CancellationToken cancellationToken)
    {
        using var activity = IdentityTelemetry.Activities.StartActivity("identity.cleanup");
        var now = clock.GetUtcNow();
        var settings = options.Value;
        var sessionCutoff = now.AddDays(-settings.SessionRetentionDays);
        var otpCutoff = now.AddDays(-settings.OtpRetentionDays);
        var eventCutoff = now.AddDays(-settings.ProcessedEventRetentionDays);
        var attemptCutoff = now.AddDays(-settings.RegistrationAttemptRetentionDays);
        var deletedAccountCutoff = now.AddDays(-settings.DeletedAccountRetentionDays);
        await using var transaction = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

        // Session deletion cascades to refresh tokens. Never prune rotated tokens of live sessions:
        // they remain necessary to detect refresh-token reuse until the session is unusable.
        var sessionIds = await db.UserSessions.Where(session => session.ExpiresAtUtc < sessionCutoff
            || (session.RevokedAtUtc != null && session.RevokedAtUtc < sessionCutoff))
            .OrderBy(session => session.ExpiresAtUtc).ThenBy(session => session.Id)
            .Select(session => session.Id).Take(settings.BatchSize).ToArrayAsync(cancellationToken);
        var sessions = await db.UserSessions.Where(session => sessionIds.Contains(session.Id)).ExecuteDeleteAsync(cancellationToken);
        var otpIds = await db.OtpChallenges.Where(otp => otp.ExpiresAtUtc < otpCutoff)
            .OrderBy(otp => otp.ExpiresAtUtc).ThenBy(otp => otp.Id).Select(otp => otp.Id)
            .Take(settings.BatchSize).ToArrayAsync(cancellationToken);
        var otps = await db.OtpChallenges.Where(otp => otpIds.Contains(otp.Id)).ExecuteDeleteAsync(cancellationToken);
        var eventIds = await db.OutboxMessages.Where(message => message.ProcessedAtUtc != null && message.ProcessedAtUtc < eventCutoff)
            .OrderBy(message => message.ProcessedAtUtc).ThenBy(message => message.Id).Select(message => message.Id)
            .Take(settings.BatchSize).ToArrayAsync(cancellationToken);
        var events = await db.OutboxMessages.Where(message => eventIds.Contains(message.Id)).ExecuteDeleteAsync(cancellationToken);
        var attemptIds = await db.RegistrationAttempts.Where(attempt => attempt.AttemptedAtUtc < attemptCutoff)
            .OrderBy(attempt => attempt.AttemptedAtUtc).ThenBy(attempt => attempt.Id).Select(attempt => attempt.Id)
            .Take(settings.BatchSize).ToArrayAsync(cancellationToken);
        var attempts = await db.RegistrationAttempts.Where(attempt => attemptIds.Contains(attempt.Id)).ExecuteDeleteAsync(cancellationToken);
        var deletedUserIds = await db.Users
            .Where(user => user.Status == AccountStatus.Deleted
                && user.DeletedAtUtc != null && user.DeletedAtUtc < deletedAccountCutoff)
            .OrderBy(user => user.DeletedAtUtc).ThenBy(user => user.Id)
            .Select(user => user.Id).Take(settings.BatchSize).ToArrayAsync(cancellationToken);
        await db.UserSessions.Where(session => deletedUserIds.Contains(session.UserId)).ExecuteDeleteAsync(cancellationToken);
        await db.UserDevices.Where(device => deletedUserIds.Contains(device.UserId)).ExecuteDeleteAsync(cancellationToken);
        await db.OtpChallenges.Where(otp => deletedUserIds.Contains(otp.UserId)).ExecuteDeleteAsync(cancellationToken);
        await db.PasswordHistory.Where(entry => deletedUserIds.Contains(entry.UserId)).ExecuteDeleteAsync(cancellationToken);
        var deletedAccounts = await db.Users.Where(user => deletedUserIds.Contains(user.Id)).ExecuteDeleteAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        logger.LogInformation("Cleanup completed: {Sessions} sessions (including refresh tokens), {Otps} OTPs, {Events} processed events, {Attempts} registration attempts, {DeletedAccounts} account tombstones.",
            sessions, otps, events, attempts, deletedAccounts);
        telemetry.Record("cleanup", "deleted", sessions + otps + events + attempts + deletedAccounts);
        return new CleanupResult(sessions, otps, events, attempts, deletedAccounts);
    }
}
