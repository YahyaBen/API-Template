using Microsoft.Extensions.Logging;
using foodiya.app.Identity.Application.Exceptions;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Domain.Enums;

namespace foodiya.app.Identity.Infrastructure.Registration;

internal sealed class SmtpIdentityEmailSender(
    IIdentityEmailTransport transport,
    ILogger<SmtpIdentityEmailSender> logger) : IIdentityEmailSender
{
    public Task SendRegistrationOtpAsync(
        string email,
        IdentityEmailContext context,
        Guid challengeId,
        string code,
        DateTimeOffset expiresAtUtc,
        CancellationToken cancellationToken) =>
        SendAsync(CreateOtpMessage(
            email,
            context,
            "Registration",
            challengeId,
            code,
            expiresAtUtc), cancellationToken);

    public Task SendAccountOtpAsync(
        string email,
        IdentityEmailContext context,
        OtpPurpose purpose,
        Guid challengeId,
        string code,
        DateTimeOffset expiresAtUtc,
        CancellationToken cancellationToken) =>
        SendAsync(purpose switch
        {
            OtpPurpose.AccountDeletion => CreateAccountDeletionMessage(
                email, context, challengeId, code, expiresAtUtc),
            OtpPurpose.DeviceLogin => CreateDeviceLoginMessage(
                email, context, challengeId, code, expiresAtUtc),
            _ => CreateOtpMessage(
                email,
                context,
                "PasswordReset",
                challengeId,
                code,
                expiresAtUtc)
        }, cancellationToken);

    public Task SendWelcomeAsync(
        string email,
        IdentityEmailContext context,
        CancellationToken cancellationToken)
    {
        var language = IdentityEmailLocalization.NormalizeLanguage(context.PreferredLanguage);
        var recipientName = GetRecipientName(context, language);
        var values = CommonValues(context, language, L(language, "Welcome_Subject"));
        values["preheader"] = L(language, "Welcome_Preheader");
        values["welcome_label"] = L(language, "Welcome_Label");
        values["heading"] = L(language, "Welcome_Title", recipientName);
        values["message"] = L(language, "Welcome_Message");
        values["security_notice"] = L(language, "Welcome_Security");
        values["footer"] = L(language, "Welcome_Footer");

        return SendAsync(new IdentityEmailMessage(
            email,
            L(language, "Welcome_Subject"),
            L(language, "Welcome_Text", recipientName),
            IdentityEmailTemplateRenderer.Render("WelcomeEmail", values)), cancellationToken);
    }

    public Task SendAccountDeletionConfirmationAsync(
        string email,
        IdentityEmailContext context,
        CancellationToken cancellationToken)
    {
        var language = IdentityEmailLocalization.NormalizeLanguage(context.PreferredLanguage);
        var recipientName = GetRecipientName(context, language);
        var values = CommonValues(
            context,
            language,
            L(language, "DeletionConfirmation_Subject"));
        values["heading"] = L(language, "DeletionConfirmation_Title");
        values["completed_message"] = L(language, "DeletionConfirmation_Completed");
        values["sessions_message"] = L(language, "DeletionConfirmation_Sessions");
        values["support_message"] = L(language, "DeletionConfirmation_Support");
        values["footer"] = L(language, "DeletionConfirmation_Footer");

        return SendAsync(new IdentityEmailMessage(
            email,
            L(language, "DeletionConfirmation_Subject"),
            L(language, "DeletionConfirmation_Text", recipientName),
            IdentityEmailTemplateRenderer.Render("AccountDeletionConfirmation", values)), cancellationToken);
    }

    private static IdentityEmailMessage CreateOtpMessage(
        string email,
        IdentityEmailContext context,
        string resourcePrefix,
        Guid challengeId,
        string code,
        DateTimeOffset expiresAtUtc)
    {
        var language = IdentityEmailLocalization.NormalizeLanguage(context.PreferredLanguage);
        var heading = L(language, $"{resourcePrefix}_Title");
        var values = CommonValues(context, language, heading);
        values["preheader"] = L(language, "Otp_Preheader", code);
        values["message"] = L(language, $"{resourcePrefix}_Message");
        values["code_label"] = L(language, "Otp_CodeLabel");
        values["safety_notice"] = L(language, "Otp_Safety");
        values["footer"] = L(language, "Otp_Footer");
        AddChallengeValues(values, challengeId, code, expiresAtUtc);

        return new IdentityEmailMessage(
            email,
            L(language, $"{resourcePrefix}_Subject"),
            BuildOtpText(language, heading, code, challengeId, expiresAtUtc),
            IdentityEmailTemplateRenderer.Render("OtpEmail", values));
    }

    private static IdentityEmailMessage CreateAccountDeletionMessage(
        string email,
        IdentityEmailContext context,
        Guid challengeId,
        string code,
        DateTimeOffset expiresAtUtc)
    {
        var language = IdentityEmailLocalization.NormalizeLanguage(context.PreferredLanguage);
        var heading = L(language, "Deletion_Title");
        var values = CommonValues(context, language, heading);
        values["preheader"] = L(language, "Deletion_Preheader");
        values["heading"] = heading;
        values["message"] = L(language, "Deletion_Message");
        values["impact_notice"] = L(language, "Deletion_Impact");
        values["code_label"] = L(language, "Deletion_CodeLabel");
        values["safety_notice"] = L(language, "Deletion_Safety");
        values["footer"] = L(language, "Deletion_Footer");
        AddChallengeValues(values, challengeId, code, expiresAtUtc);

        return new IdentityEmailMessage(
            email,
            L(language, "Deletion_Subject"),
            BuildOtpText(language, heading, code, challengeId, expiresAtUtc),
            IdentityEmailTemplateRenderer.Render("AccountDeletion", values));
    }

    private static IdentityEmailMessage CreateDeviceLoginMessage(
        string email,
        IdentityEmailContext context,
        Guid challengeId,
        string code,
        DateTimeOffset expiresAtUtc)
    {
        var language = IdentityEmailLocalization.NormalizeLanguage(context.PreferredLanguage);
        var heading = L(language, "Device_Title");
        var values = CommonValues(context, language, heading);
        values["preheader"] = L(language, "Device_Preheader");
        values["heading"] = heading;
        values["message"] = L(language, "Device_Message");
        values["device_label"] = L(language, "Device_Label");
        values["app_version_label"] = L(language, "Device_AppVersion");
        values["connected_label"] = L(language, "Device_Connected");
        values["code_label"] = L(language, "Device_CodeLabel");
        values["safety_notice"] = L(language, "Device_Safety");
        values["footer"] = L(language, "Device_Footer");
        values["device_name"] = context.DeviceName ?? L(language, "Common_UnknownDevice");
        values["app_version"] = context.AppVersion ?? L(language, "Common_UnknownValue");
        values["connected_at"] = (context.ConnectedAtUtc ?? DateTimeOffset.UtcNow).ToString("O");
        AddChallengeValues(values, challengeId, code, expiresAtUtc);

        return new IdentityEmailMessage(
            email,
            L(language, "Device_Subject"),
            BuildOtpText(language, heading, code, challengeId, expiresAtUtc),
            IdentityEmailTemplateRenderer.Render("NewDeviceLogin", values));
    }

    private static Dictionary<string, string> CommonValues(
        IdentityEmailContext context,
        string language,
        string title)
    {
        var recipientName = GetRecipientName(context, language);
        return new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["language_code"] = language,
            ["direction"] = IdentityEmailLocalization.Direction(language),
            ["text_align"] = IdentityEmailLocalization.TextAlign(language),
            ["title"] = title,
            ["recipient_name"] = recipientName,
            ["greeting"] = L(language, "Common_Greeting", recipientName),
            ["challenge_reference_label"] = L(language, "Common_ChallengeReference"),
            ["expires_label"] = L(language, "Common_Expires"),
            ["identity_footer"] = L(language, "Common_IdentityFooter")
        };
    }

    private static void AddChallengeValues(
        IDictionary<string, string> values,
        Guid challengeId,
        string code,
        DateTimeOffset expiresAtUtc)
    {
        values["code"] = code;
        values["challenge_id"] = challengeId.ToString("D");
        values["expires_at"] = expiresAtUtc.ToString("O");
    }

    private static string GetRecipientName(IdentityEmailContext context, string language) =>
        string.IsNullOrWhiteSpace(context.RecipientName)
            ? L(language, "Common_RecipientFallback")
            : context.RecipientName.Trim();

    private static string BuildOtpText(
        string language,
        string purpose,
        string code,
        Guid challengeId,
        DateTimeOffset expiresAtUtc) =>
        L(language, "Otp_Text", purpose, code, challengeId.ToString("D"), expiresAtUtc.ToString("O"));

    private static string L(string language, string key, params object[] arguments) =>
        IdentityEmailLocalization.Get(language, key, arguments);

    private async Task SendAsync(IdentityEmailMessage message, CancellationToken cancellationToken)
    {
        try
        {
            await transport.SendAsync(message, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogError("SMTP identity email delivery failed ({FailureType}).", exception.GetType().Name);
            throw new IdentityDeliveryException(
                "EMAIL_DELIVERY_FAILED",
                "The identity email could not be delivered. Please try again later.");
        }
    }
}
