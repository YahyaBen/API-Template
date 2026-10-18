namespace foodiya.app.Identity.Application.Interfaces;

public sealed record IdentityEmailContext(
    string? RecipientName = null,
    string? PreferredLanguage = null,
    string? DeviceName = null,
    string? AppVersion = null,
    DateTimeOffset? ConnectedAtUtc = null);

public interface IIdentityEmailSender
{
    Task SendAccountOtpAsync(
        string email,
        IdentityEmailContext context,
        foodiya.app.Identity.Domain.Enums.OtpPurpose purpose,
        Guid challengeId,
        string code,
        DateTimeOffset expiresAtUtc,
        CancellationToken cancellationToken);

    Task SendRegistrationOtpAsync(
        string email,
        IdentityEmailContext context,
        Guid challengeId,
        string code,
        DateTimeOffset expiresAtUtc,
        CancellationToken cancellationToken);

    Task SendWelcomeAsync(
        string email,
        IdentityEmailContext context,
        CancellationToken cancellationToken);

    Task SendAccountDeletionConfirmationAsync(
        string email,
        IdentityEmailContext context,
        CancellationToken cancellationToken);
}
