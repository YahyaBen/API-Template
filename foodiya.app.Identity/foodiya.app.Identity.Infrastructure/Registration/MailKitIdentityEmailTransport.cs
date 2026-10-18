using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using foodiya.app.Identity.Infrastructure.Options;

namespace foodiya.app.Identity.Infrastructure.Registration;

internal sealed record IdentityEmailMessage(
    string Recipient,
    string Subject,
    string TextBody,
    string HtmlBody);

internal interface IIdentityEmailTransport
{
    Task SendAsync(IdentityEmailMessage message, CancellationToken cancellationToken);
}

internal sealed class MailKitIdentityEmailTransport(IOptions<EmailDeliveryOptions> options)
    : IIdentityEmailTransport
{
    public async Task SendAsync(IdentityEmailMessage email, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var message = new MimeMessage();
        message.From.Add(MailboxAddress.Parse(settings.FromAddress));
        message.To.Add(MailboxAddress.Parse(email.Recipient));
        message.Subject = email.Subject;
        message.Body = new BodyBuilder
        {
            TextBody = email.TextBody,
            HtmlBody = email.HtmlBody
        }.ToMessageBody();

        using var client = new SmtpClient { Timeout = checked(settings.TimeoutSeconds * 1000) };
        await client.ConnectAsync(settings.Host, settings.Port,
            settings.UseStartTls ? SecureSocketOptions.StartTls : SecureSocketOptions.Auto, cancellationToken);
        if (!string.IsNullOrWhiteSpace(settings.UserName))
        {
            await client.AuthenticateAsync(settings.UserName, settings.Password, cancellationToken);
        }

        await client.SendAsync(message, cancellationToken);
        await client.DisconnectAsync(true, cancellationToken);
    }
}
