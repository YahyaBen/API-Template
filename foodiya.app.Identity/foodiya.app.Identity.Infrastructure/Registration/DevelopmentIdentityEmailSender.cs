using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using foodiya.app.Identity.Application.Exceptions;
using foodiya.app.Identity.Application.Interfaces;

namespace foodiya.app.Identity.Infrastructure.Registration;

internal sealed class DevelopmentIdentityEmailSender(
    IHostEnvironment environment,
    IOptions<foodiya.app.Identity.Infrastructure.Options.EmailDeliveryOptions> options,
    ILogger<DevelopmentIdentityEmailSender> logger) : IIdentityEmailSender
{
    public Task SendAccountOtpAsync(
        string email,
        IdentityEmailContext context,
        foodiya.app.Identity.Domain.Enums.OtpPurpose purpose,
        Guid challengeId,
        string code,
        DateTimeOffset expiresAtUtc,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (!environment.IsDevelopment() || !options.Value.LogDevelopmentCodes)
        {
            throw new IdentityDeliveryException(
                "EMAIL_PROVIDER_NOT_CONFIGURED",
                "The identity email provider is not configured.");
        }

        logger.LogWarning(
            "Development {Purpose} OTP for {Email}: {OtpCode}. Challenge {ChallengeId}, expires at {ExpiresAtUtc}.",
            purpose, email, code, challengeId, expiresAtUtc);
        return Task.CompletedTask;
    }

    public Task SendRegistrationOtpAsync(
        string email,
        IdentityEmailContext context,
        Guid challengeId,
        string code,
        DateTimeOffset expiresAtUtc,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!environment.IsDevelopment() || !options.Value.LogDevelopmentCodes)
        {
            throw new IdentityDeliveryException(
                "EMAIL_PROVIDER_NOT_CONFIGURED",
                "The identity email provider is not configured.");
        }

        logger.LogWarning(
            "Development registration OTP for {Email}: {OtpCode}. Challenge {ChallengeId}, expires at {ExpiresAtUtc}.",
            email,
            code,
            challengeId,
            expiresAtUtc);

        return Task.CompletedTask;
    }

    public Task SendWelcomeAsync(
        string email,
        IdentityEmailContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation("Development welcome email for {Email}.", email);
        return Task.CompletedTask;
    }

    public Task SendAccountDeletionConfirmationAsync(
        string email,
        IdentityEmailContext context,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        logger.LogInformation("Development account-deletion confirmation email for {Email}.", email);
        return Task.CompletedTask;
    }
}
