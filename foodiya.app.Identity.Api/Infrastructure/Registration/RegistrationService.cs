using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using foodiya.app.Identity.Application.Exceptions;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Contracts.Registration;
using foodiya.app.Identity.Domain.Constants;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Domain.Enums;
using foodiya.app.Identity.Infrastructure.Identity;
using foodiya.app.Identity.Infrastructure.Options;
using foodiya.app.Identity.Infrastructure.Persistence;

namespace foodiya.app.Identity.Infrastructure.Registration;

internal sealed class RegistrationService(
    UserManager<ApplicationUser> userManager,
    IdentityDataContext dataContext,
    IOtpCodeService otpCodeService,
    IIdentityEmailSender emailSender,
    IOptions<RegistrationOptions> options,
    TimeProvider timeProvider,
    ILogger<RegistrationService> logger) : IRegistrationService
{
    private readonly RegistrationOptions _options = options.Value;

    public async Task<RegistrationChallengeResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var userName = request.UserName.Trim();
        var normalizedEmail = userManager.NormalizeEmail(email);
        await using var transaction = await dataContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            throw new IdentityConflictException(
                "EMAIL_ALREADY_REGISTERED",
                "An account already exists for this email address.");
        }

        if (await userManager.FindByNameAsync(userName) is not null)
        {
            throw new IdentityConflictException(
                "USERNAME_ALREADY_REGISTERED",
                "This user name is already in use.");
        }

        var now = timeProvider.GetUtcNow();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = userName,
            Email = email,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            PreferredLanguage = NormalizeLanguage(request.PreferredLanguage),
            Status = AccountStatus.Pending,
            CreatedAtUtc = now,
            LockoutEnabled = true,
            SecurityStamp = Guid.NewGuid().ToString("N")
        };

        EnsureSucceeded(
            await userManager.CreateAsync(user, request.Password),
            "REGISTRATION_REJECTED");
        EnsureSucceeded(
            await userManager.AddToRoleAsync(user, RoleNames.User),
            "ROLE_ASSIGNMENT_FAILED");

        var (challenge, code) = await IssueChallengeAsync(
            user.Id,
            normalizedEmail,
            now,
            cancellationToken);

        dataContext.OutboxMessages.Add(new OutboxMessage
        {
            OccurredAtUtc = now,
            Type = "UserRegistrationStarted",
            Payload = JsonSerializer.Serialize(new { user.Id, user.Email })
        });

        await dataContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await emailSender.SendRegistrationOtpAsync(
            email,
            ToEmailContext(user),
            challenge.Id,
            code,
            challenge.ExpiresAtUtc,
            cancellationToken);
        return ToChallengeResponse(challenge, email);
    }

    public async Task<RegistrationConfirmationResponse> ConfirmAsync(
        ConfirmRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        var now = timeProvider.GetUtcNow();
        await using var transaction = await dataContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var challenge = await dataContext.OtpChallenges.SingleOrDefaultAsync(
            candidate => candidate.Id == request.ChallengeId
                && candidate.Purpose == OtpPurpose.Registration,
            cancellationToken);

        if (challenge is null || !challenge.IsUsable(now))
        {
            throw new IdentityValidationException(
                "OTP_INVALID_OR_EXPIRED",
                "The confirmation code is invalid or expired.");
        }

        if (!otpCodeService.VerifyCode(challenge.Id, request.Code, challenge.CodeHash))
        {
            challenge.FailedAttempts++;
            if (challenge.FailedAttempts >= challenge.MaxAttempts)
            {
                challenge.InvalidatedAtUtc = now;
            }

            await dataContext.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            throw new IdentityValidationException(
                "OTP_INVALID_OR_EXPIRED",
                "The confirmation code is invalid or expired.");
        }

        var user = await userManager.FindByIdAsync(challenge.UserId.ToString())
            ?? throw new IdentityValidationException(
                "REGISTRATION_NOT_FOUND",
                "The pending registration could not be found.");

        if (user.Status != AccountStatus.Pending)
        {
            throw new IdentityConflictException(
                "ACCOUNT_ALREADY_CONFIRMED",
                "The account is already confirmed.");
        }

        challenge.ConsumedAtUtc = now;
        user.EmailConfirmed = true;
        user.Status = AccountStatus.Active;
        user.UpdatedAtUtc = now;

        EnsureSucceeded(await userManager.UpdateAsync(user), "REGISTRATION_CONFIRMATION_FAILED");
        dataContext.OutboxMessages.Add(new OutboxMessage
        {
            OccurredAtUtc = now,
            Type = "UserRegistered",
            Payload = JsonSerializer.Serialize(new { user.Id, user.Email, user.UserName, user.FirstName, user.LastName })
        });

        await dataContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await SendNotificationSafelyAsync(
            () => emailSender.SendWelcomeAsync(
                user.Email ?? string.Empty,
                ToEmailContext(user),
                cancellationToken),
            "welcome",
            user.Id);

        return new RegistrationConfirmationResponse(
            user.Id,
            user.Email ?? string.Empty,
            user.EmailConfirmed,
            user.Status.ToString());
    }

    public async Task<RegistrationChallengeResponse> ResendAsync(
        ResendRegistrationOtpRequest request,
        CancellationToken cancellationToken)
    {
        var email = request.Email.Trim();
        var normalizedEmail = userManager.NormalizeEmail(email);
        var user = await userManager.FindByEmailAsync(email);

        if (user is null || user.Status != AccountStatus.Pending)
        {
            throw new IdentityValidationException(
                "PENDING_REGISTRATION_NOT_FOUND",
                "No pending registration exists for this email address.");
        }

        var now = timeProvider.GetUtcNow();
        await using var transaction = await dataContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        var latestChallenge = await dataContext.OtpChallenges
            .Where(challenge => challenge.UserId == user.Id
                && challenge.Purpose == OtpPurpose.Registration)
            .OrderByDescending(challenge => challenge.CreatedAtUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (latestChallenge is not null)
        {
            var resendAvailableAt = latestChallenge.CreatedAtUtc.AddSeconds(
                _options.OtpResendCooldownSeconds);
            if (resendAvailableAt > now)
            {
                throw new IdentityThrottleException(
                    "OTP_RESEND_TOO_SOON",
                    "A new confirmation code cannot be requested yet.",
                    resendAvailableAt);
            }
        }

        var (challenge, code) = await IssueChallengeAsync(
            user.Id,
            normalizedEmail,
            now,
            cancellationToken);
        await dataContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        await emailSender.SendRegistrationOtpAsync(
            email,
            ToEmailContext(user),
            challenge.Id,
            code,
            challenge.ExpiresAtUtc,
            cancellationToken);
        return ToChallengeResponse(challenge, email);
    }

    private async Task<(OtpChallenge Challenge, string Code)> IssueChallengeAsync(
        Guid userId,
        string normalizedEmail,
        DateTimeOffset now,
        CancellationToken cancellationToken)
    {
        await dataContext.OtpChallenges
            .Where(challenge => challenge.UserId == userId
                && challenge.Purpose == OtpPurpose.Registration
                && challenge.ConsumedAtUtc == null
                && challenge.InvalidatedAtUtc == null)
            .ExecuteUpdateAsync(
                updates => updates.SetProperty(
                    challenge => challenge.InvalidatedAtUtc,
                    now),
                cancellationToken);

        var challenge = new OtpChallenge
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            NormalizedEmail = normalizedEmail,
            CodeHash = string.Empty,
            Purpose = OtpPurpose.Registration,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.AddMinutes(_options.OtpLifetimeMinutes),
            MaxAttempts = _options.OtpMaxAttempts
        };
        var code = otpCodeService.GenerateCode();
        challenge.CodeHash = otpCodeService.HashCode(challenge.Id, code);
        dataContext.OtpChallenges.Add(challenge);
        return (challenge, code);
    }

    private RegistrationChallengeResponse ToChallengeResponse(
        OtpChallenge challenge,
        string email) => new(
            challenge.Id,
            MaskEmail(email),
            challenge.ExpiresAtUtc,
            challenge.CreatedAtUtc.AddSeconds(_options.OtpResendCooldownSeconds));

    private static string NormalizeLanguage(string language) =>
        IdentityEmailLocalization.NormalizeLanguage(language);

    private static IdentityEmailContext ToEmailContext(ApplicationUser user) =>
        new(user.FirstName, user.PreferredLanguage);

    private async Task SendNotificationSafelyAsync(Func<Task> send, string notification, Guid userId)
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
                "Post-commit identity {Notification} email failed for user {UserId} ({FailureType}).",
                notification,
                userId,
                exception.GetType().Name);
        }
    }

    internal static string MaskEmail(string email)
    {
        var separator = email.IndexOf('@');
        if (separator < 0)
        {
            return "***";
        }

        if (separator <= 1)
        {
            return $"***{email[separator..]}";
        }

        return $"{email[0]}***{email[(separator - 1)..]}";
    }

    private static void EnsureSucceeded(IdentityResult result, string code)
    {
        if (result.Succeeded)
        {
            return;
        }

        var message = string.Join(" ", result.Errors.Select(error => error.Description));
        throw new IdentityValidationException(code, message);
    }
}
