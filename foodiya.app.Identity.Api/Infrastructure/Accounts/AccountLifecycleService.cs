using System.Data;
using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using foodiya.app.Identity.Application.Exceptions;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Contracts.Accounts;
using foodiya.app.Identity.Domain.Constants;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Domain.Enums;
using foodiya.app.Identity.Infrastructure.Authorization;
using foodiya.app.Identity.Infrastructure.Identity;
using foodiya.app.Identity.Infrastructure.Options;
using foodiya.app.Identity.Infrastructure.Persistence;

namespace foodiya.app.Identity.Infrastructure.Accounts;

internal sealed class AccountLifecycleService(
    UserManager<ApplicationUser> userManager,
    IdentityDataContext dataContext,
    IAccessSessionValidator accessSessionValidator,
    IOtpCodeService otpCodes,
    IIdentityEmailSender emailSender,
    IOptions<RegistrationOptions> otpOptions,
    IOptions<AccountLifecycleOptions> accountOptions,
    TimeProvider timeProvider,
    ILogger<AccountLifecycleService> logger) : IAccountLifecycleService
{
    private static readonly PasswordRecoveryResponse RecoveryAccepted = new(
        "If the account is eligible, password recovery instructions will be sent to its email address.");

    public async Task<PasswordRecoveryResponse> ForgotPasswordAsync(
        ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await BeginTransactionAsync(cancellationToken);
        var user = await userManager.FindByEmailAsync(request.Email.Trim());
        if (user is null || !IsActive(user))
        {
            return RecoveryAccepted;
        }

        var challenge = await IssueChallengeAsync(user, OtpPurpose.PasswordReset, cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        if (challenge is not null)
        {
            try
            {
                await SendChallengeAsync(user, challenge.Value, cancellationToken);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                // Delivery failures must not turn the public response into an account-existence oracle.
                logger.LogError("Password recovery delivery failed ({FailureType}).", exception.GetType().Name);
            }
        }

        return RecoveryAccepted;
    }

    public async Task ResetPasswordAsync(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        await using var transaction = await BeginTransactionAsync(cancellationToken);
        var challenge = await VerifyChallengeAsync(
            request.ChallengeId, request.Code, OtpPurpose.PasswordReset, null, transaction, cancellationToken);
        var user = await userManager.FindByIdAsync(challenge.UserId.ToString());
        if (user is null || !IsActive(user)
            || user.NormalizedEmail != challenge.NormalizedEmail)
        {
            throw InvalidChallenge();
        }

        await SetPasswordAsync(user, request.NewPassword, "PasswordReset", cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task ChangePasswordAsync(
        ClaimsPrincipal principal,
        ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await BeginTransactionAsync(cancellationToken);
        var (user, _) = await GetAuthenticatedUserAsync(principal, cancellationToken);
        await CheckPasswordAsync(user, request.CurrentPassword, transaction, cancellationToken);
        await SetPasswordAsync(user, request.NewPassword, "PasswordChanged", cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task LogoutAsync(ClaimsPrincipal principal, bool allSessions, CancellationToken cancellationToken)
    {
        await using var transaction = await BeginTransactionAsync(cancellationToken);
        var (user, sessionId) = await GetAuthenticatedUserAsync(principal, cancellationToken);
        var reason = allSessions ? "LoggedOutEverywhere" : "LoggedOut";
        if (allSessions)
        {
            RotateSecurityStamp(user);
            EnsureSucceeded(await userManager.UpdateAsync(user));
        }

        await RevokeSessionsAsync(user.Id, allSessions ? null : sessionId, reason, cancellationToken);
        AddEvent(reason, user.Id, allSessions ? null : sessionId);
        await dataContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    public async Task<AccountChallengeResponse> RequestDeletionAsync(
        ClaimsPrincipal principal,
        RequestAccountDeletionRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await BeginTransactionAsync(cancellationToken);
        var (user, _) = await GetAuthenticatedUserAsync(principal, cancellationToken);
        await CheckPasswordAsync(user, request.CurrentPassword, transaction, cancellationToken);
        await ProtectFinalSuperAdminAsync(user);
        var issued = await IssueChallengeAsync(user, OtpPurpose.AccountDeletion, cancellationToken);
        if (issued is null)
        {
            throw new IdentityThrottleException(
                "OTP_RESEND_TOO_SOON", "Wait before requesting another deletion code.",
                timeProvider.GetUtcNow().AddSeconds(otpOptions.Value.OtpResendCooldownSeconds));
        }

        await transaction.CommitAsync(cancellationToken);
        await SendChallengeAsync(user, issued.Value, cancellationToken);
        return new AccountChallengeResponse(issued.Value.Challenge.Id, issued.Value.Challenge.ExpiresAtUtc);
    }

    public async Task ConfirmDeletionAsync(
        ClaimsPrincipal principal,
        ConfirmAccountDeletionRequest request,
        CancellationToken cancellationToken)
    {
        await using var transaction = await BeginTransactionAsync(cancellationToken);
        var (user, _) = await GetAuthenticatedUserAsync(principal, cancellationToken);
        var challenge = await VerifyChallengeAsync(
            request.ChallengeId, request.Code, OtpPurpose.AccountDeletion, user.Id, transaction, cancellationToken);
        if (user.NormalizedEmail != challenge.NormalizedEmail)
        {
            throw InvalidChallenge();
        }

        await ProtectFinalSuperAdminAsync(user);
        var notificationEmail = user.Email ?? string.Empty;
        var notificationContext = ToEmailContext(user);
        var now = timeProvider.GetUtcNow();
        user.Status = AccountStatus.Deleted;
        user.DeletedAtUtc = now;
        user.Email = $"deleted-{user.Id:N}@deleted.invalid";
        user.UserName = $"deleted-{user.Id:N}";
        user.FirstName = "Deleted";
        user.LastName = "Account";
        user.PreferredLanguage = "en";
        user.PhoneNumber = null;
        user.PhoneNumberConfirmed = false;
        user.EmailConfirmed = false;
        user.PasswordHash = null;
        user.TwoFactorEnabled = false;
        RotateSecurityStamp(user);
        EnsureSucceeded(await userManager.UpdateAsync(user));

        await RevokeSessionsAsync(user.Id, null, "AccountDeleted", cancellationToken);
        await dataContext.UserDevices.Where(device => device.UserId == user.Id)
            .ExecuteUpdateAsync(updates => updates
                .SetProperty(device => device.RevokedAtUtc, now)
                .SetProperty(device => device.RevocationReason, "AccountDeleted")
                .SetProperty(device => device.FirebaseInstallationId, (string?)null)
                .SetProperty(device => device.FirebaseInstallationUpdatedAtUtc, now)
                .SetProperty(device => device.DeviceName, (string?)null)
                .SetProperty(device => device.LastIpAddress, (string?)null), cancellationToken);
        await dataContext.UserLogins.Where(login => login.UserId == user.Id).ExecuteDeleteAsync(cancellationToken);
        await dataContext.UserTokens.Where(token => token.UserId == user.Id).ExecuteDeleteAsync(cancellationToken);
        await dataContext.UserClaims.Where(claim => claim.UserId == user.Id).ExecuteDeleteAsync(cancellationToken);
        await dataContext.UserRoles.Where(role => role.UserId == user.Id).ExecuteDeleteAsync(cancellationToken);
        await dataContext.PasswordHistory.Where(entry => entry.UserId == user.Id).ExecuteDeleteAsync(cancellationToken);
        await dataContext.OtpChallenges.Where(otp => otp.UserId == user.Id).ExecuteDeleteAsync(cancellationToken);
        // VerifyChallenge marks the row consumed; it was just deleted in the same transaction.
        dataContext.Entry(challenge).State = EntityState.Detached;
        AddEvent("UserDeleted", user.Id);
        await dataContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await SendNotificationSafelyAsync(
            () => emailSender.SendAccountDeletionConfirmationAsync(
                notificationEmail,
                notificationContext,
                cancellationToken),
            user.Id);
    }

    private async Task SetPasswordAsync(
        ApplicationUser user, string password, string reason, CancellationToken cancellationToken)
    {
        // Validate before retaining the old hash or changing any credentials.
        foreach (var validator in userManager.PasswordValidators)
        {
            EnsureSucceeded(await validator.ValidateAsync(userManager, user, password));
        }

        var history = await dataContext.PasswordHistory.Where(entry => entry.UserId == user.Id)
            .OrderByDescending(entry => entry.CreatedAtUtc).ThenBy(entry => entry.Id)
            .ToListAsync(cancellationToken);
        var hashes = history.Take(accountOptions.Value.PasswordHistoryCount).Select(entry => entry.PasswordHash);
        if (user.PasswordHash is not null)
        {
            hashes = hashes.Prepend(user.PasswordHash);
        }

        if (hashes.Any(hash => userManager.PasswordHasher.VerifyHashedPassword(user, hash, password)
            != PasswordVerificationResult.Failed))
        {
            throw new IdentityValidationException("PASSWORD_REUSED", "Choose a password that you have not used recently.");
        }

        var now = timeProvider.GetUtcNow();
        if (user.PasswordHash is not null)
        {
            dataContext.PasswordHistory.Add(new PasswordHistoryEntry
            {
                UserId = user.Id,
                PasswordHash = user.PasswordHash,
                CreatedAtUtc = now
            });
        }

        dataContext.PasswordHistory.RemoveRange(history.Skip(accountOptions.Value.PasswordHistoryCount - 1));
        user.PasswordHash = userManager.PasswordHasher.HashPassword(user, password);
        user.AccessFailedCount = 0;
        user.LockoutEnd = null;
        RotateSecurityStamp(user);
        EnsureSucceeded(await userManager.UpdateAsync(user));
        await RevokeSessionsAsync(user.Id, null, reason, cancellationToken);
        await InvalidateChallengesAsync(user.Id, null, cancellationToken);
        AddEvent(reason, user.Id);
        await dataContext.SaveChangesAsync(cancellationToken);
    }

    private async Task CheckPasswordAsync(
        ApplicationUser user, string password, IDbContextTransaction transaction, CancellationToken cancellationToken)
    {
        if (await userManager.IsLockedOutAsync(user))
        {
            throw new IdentityLockedException("ACCOUNT_LOCKED", "The account is temporarily locked.",
                user.LockoutEnd ?? timeProvider.GetUtcNow().AddMinutes(15));
        }

        if (!await userManager.CheckPasswordAsync(user, password))
        {
            EnsureSucceeded(await userManager.AccessFailedAsync(user));
            // Persist the failed attempt even though the requested operation is rejected.
            await transaction.CommitAsync(cancellationToken);
            throw new IdentityUnauthorizedException("INVALID_CREDENTIALS", "The current password is incorrect.");
        }

        EnsureSucceeded(await userManager.ResetAccessFailedCountAsync(user));
    }

    private async Task<(ApplicationUser User, Guid SessionId)> GetAuthenticatedUserAsync(
        ClaimsPrincipal principal, CancellationToken cancellationToken)
    {
        var access = await accessSessionValidator.ValidateAsync(principal, cancellationToken)
            ?? throw new IdentityUnauthorizedException("ACCESS_SESSION_INVALID", "The authenticated session is no longer valid.");
        var user = await userManager.FindByIdAsync(access.User.Id.ToString())
            ?? throw new IdentityUnauthorizedException("ACCESS_SESSION_INVALID", "The account no longer exists.");
        return (user, access.Session.Id);
    }

    private async Task<(OtpChallenge Challenge, string Code)?> IssueChallengeAsync(
        ApplicationUser user, OtpPurpose purpose, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        var latest = await dataContext.OtpChallenges.Where(otp => otp.UserId == user.Id && otp.Purpose == purpose)
            .OrderByDescending(otp => otp.CreatedAtUtc).FirstOrDefaultAsync(cancellationToken);
        if (latest is not null && latest.CreatedAtUtc.AddSeconds(otpOptions.Value.OtpResendCooldownSeconds) > now)
        {
            return null;
        }

        await InvalidateChallengesAsync(user.Id, purpose, cancellationToken);
        var challenge = new OtpChallenge
        {
            UserId = user.Id,
            Purpose = purpose,
            NormalizedEmail = user.NormalizedEmail!,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddMinutes(otpOptions.Value.OtpLifetimeMinutes),
            MaxAttempts = otpOptions.Value.OtpMaxAttempts,
            CodeHash = string.Empty
        };
        var code = otpCodes.GenerateCode();
        challenge.CodeHash = otpCodes.HashCode(challenge.Id, code);
        dataContext.OtpChallenges.Add(challenge);
        await dataContext.SaveChangesAsync(cancellationToken);
        return (challenge, code);
    }

    private async Task<OtpChallenge> VerifyChallengeAsync(
        Guid challengeId, string code, OtpPurpose purpose, Guid? userId,
        IDbContextTransaction transaction, CancellationToken cancellationToken)
    {
        var challenge = await dataContext.OtpChallenges.SingleOrDefaultAsync(
            otp => otp.Id == challengeId && otp.Purpose == purpose, cancellationToken);
        var now = timeProvider.GetUtcNow();
        if (challenge is null || (userId.HasValue && challenge.UserId != userId.Value) || !challenge.IsUsable(now))
        {
            throw InvalidChallenge();
        }

        if (!otpCodes.VerifyCode(challenge.Id, code, challenge.CodeHash))
        {
            challenge.FailedAttempts++;
            if (challenge.FailedAttempts >= challenge.MaxAttempts)
            {
                challenge.InvalidatedAtUtc = now;
            }

            await dataContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            throw InvalidChallenge();
        }

        challenge.ConsumedAtUtc = now;
        return challenge;
    }

    private Task SendChallengeAsync(ApplicationUser user, (OtpChallenge Challenge, string Code) issued, CancellationToken cancellationToken) =>
        emailSender.SendAccountOtpAsync(user.Email!, ToEmailContext(user), issued.Challenge.Purpose,
            issued.Challenge.Id, issued.Code, issued.Challenge.ExpiresAtUtc, cancellationToken);

    private static IdentityEmailContext ToEmailContext(ApplicationUser user) =>
        new(user.FirstName, user.PreferredLanguage);

    private async Task SendNotificationSafelyAsync(Func<Task> send, Guid userId)
    {
        try
        {
            await send();
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError(
                "Post-commit account-deletion email failed for user {UserId} ({FailureType}).",
                userId,
                exception.GetType().Name);
        }
    }

    private async Task ProtectFinalSuperAdminAsync(ApplicationUser user)
    {
        if (await userManager.IsInRoleAsync(user, RoleNames.SuperAdmin)
            && (await userManager.GetUsersInRoleAsync(RoleNames.SuperAdmin)).Count <= 1)
        {
            throw new IdentityConflictException("LAST_SUPER_ADMIN_REQUIRED", "Assign another super administrator before deleting this account.");
        }
    }

    private async Task RevokeSessionsAsync(Guid userId, Guid? sessionId, string reason, CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        await dataContext.UserSessions
            .Where(session => session.UserId == userId && session.RevokedAtUtc == null
                && (!sessionId.HasValue || session.Id == sessionId.Value))
            .ExecuteUpdateAsync(updates => updates.SetProperty(session => session.RevokedAtUtc, now)
                .SetProperty(session => session.RevocationReason, reason), cancellationToken);
        await dataContext.RefreshTokens
            .Where(token => token.UserId == userId && token.RevokedAtUtc == null
                && (!sessionId.HasValue || token.SessionId == sessionId.Value))
            .ExecuteUpdateAsync(updates => updates.SetProperty(token => token.RevokedAtUtc, now)
                .SetProperty(token => token.RevocationReason, reason), cancellationToken);
    }

    private Task<int> InvalidateChallengesAsync(Guid userId, OtpPurpose? purpose, CancellationToken cancellationToken) =>
        dataContext.OtpChallenges.Where(otp => otp.UserId == userId && otp.ConsumedAtUtc == null
            && otp.InvalidatedAtUtc == null && (!purpose.HasValue || otp.Purpose == purpose.Value))
            .ExecuteUpdateAsync(updates => updates.SetProperty(otp => otp.InvalidatedAtUtc, timeProvider.GetUtcNow()), cancellationToken);

    private void RotateSecurityStamp(ApplicationUser user)
    {
        user.SecurityStamp = Guid.NewGuid().ToString("N");
        user.UpdatedAtUtc = timeProvider.GetUtcNow();
    }

    private void AddEvent(string type, Guid userId, Guid? sessionId = null) => dataContext.OutboxMessages.Add(new OutboxMessage
    {
        OccurredAtUtc = timeProvider.GetUtcNow(),
        Type = type,
        Payload = JsonSerializer.Serialize(new { UserId = userId, SessionId = sessionId })
    });

    private Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken cancellationToken) =>
        dataContext.Database.BeginTransactionAsync(IsolationLevel.Serializable, cancellationToken);

    private static bool IsActive(ApplicationUser user) => user.Status == AccountStatus.Active && user.EmailConfirmed;

    private static IdentityValidationException InvalidChallenge() =>
        new("OTP_INVALID_OR_EXPIRED", "The confirmation code is invalid or expired.");

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new IdentityValidationException("ACCOUNT_UPDATE_REJECTED",
                string.Join(" ", result.Errors.Select(error => error.Description)));
        }
    }
}
