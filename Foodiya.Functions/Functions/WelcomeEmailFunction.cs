using Foodiya.Functions.Models;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace Foodiya.Functions.Functions;

/// <summary>
/// 📧 Queue-triggered function that processes welcome-email messages.
///
/// HOW IT WORKS:
///   1. Your Foodiya.API enqueues a JSON <see cref="WelcomeEmailMessage"/>
///      into the Azure Storage Queue named "welcome-emails" right after
///      a new user registers.
///   2. This function picks up the message automatically and sends
///      a welcome email.
///
/// HOW TO ENQUEUE FROM YOUR API:
///   <code>
///   // In your AuthService / registration endpoint:
///   var queueClient = new QueueClient(connectionString, "welcome-emails");
///   await queueClient.CreateIfNotExistsAsync();
///   var message = new WelcomeEmailMessage
///   {
///       UserId   = newUser.Id,
///       Email    = newUser.Email,
///       FirstName = newUser.FirstName,
///       UserName = newUser.UserName,
///       RegisteredAtUtc = DateTime.UtcNow
///   };
///   var json = JsonSerializer.Serialize(message);
///   var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
///   await queueClient.SendMessageAsync(base64);
///   </code>
///
/// CONFIGURATION:
///   • "AzureWebJobsStorage" in local.settings.json / App Settings
///     must point to the same Storage Account the API writes to.
///   • In production, swap the fake email logic below with a real
///     provider (SendGrid, Azure Communication Services, Mailjet, etc.).
/// </summary>
public sealed class WelcomeEmailFunction
{
    private readonly ILogger<WelcomeEmailFunction> _logger;

    public WelcomeEmailFunction(ILogger<WelcomeEmailFunction> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Triggered automatically whenever a message appears in the
    /// "welcome-emails" queue.
    /// </summary>
    [Function(nameof(WelcomeEmailFunction))]
    public async Task RunAsync(
        [QueueTrigger("welcome-emails")] WelcomeEmailMessage message,
        CancellationToken ct)
    {
        _logger.LogInformation(
            "📧 Processing welcome email for UserId={UserId}, Email={Email}",
            message.UserId, message.Email);

        try
        {
            // ──────────────────────────────────────────────────
            // 🔌  Replace this block with your real email provider.
            //     Examples:
            //       • SendGrid:  await sendGridClient.SendEmailAsync(msg);
            //       • Azure Communication Services
            //       • Mailjet / Mailgun / AWS SES
            // ──────────────────────────────────────────────────

            var emailBody = BuildWelcomeEmailHtml(message.FirstName, message.UserName);

            // Simulate sending (remove this in production)
            await Task.Delay(200, ct);

            _logger.LogInformation(
                "✅ Welcome email sent successfully to {Email} (UserId={UserId})",
                message.Email, message.UserId);

            // Optional: you could output a result to another queue for auditing
            var result = new WelcomeEmailResult
            {
                UserId = message.UserId,
                Email = message.Email,
                Success = true,
                ProcessedAtUtc = DateTime.UtcNow
            };

            _logger.LogInformation("📋 Email result: {@Result}", result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "❌ Failed to send welcome email to {Email} (UserId={UserId})",
                message.Email, message.UserId);

            // The message will automatically be retried by the Queue trigger
            // (default: 5 retries with exponential back-off, then moved to
            // the "welcome-emails-poison" poison queue).
            throw;
        }
    }

    // ── Email template ───────────────────────────────────────

    private static string BuildWelcomeEmailHtml(string firstName, string userName)
    {
        return $"""
            <!DOCTYPE html>
            <html>
            <head><meta charset="utf-8" /></head>
            <body style="font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px;">
                <div style="background: linear-gradient(135deg, #ff6b35, #f7931e); padding: 30px; border-radius: 12px 12px 0 0; text-align: center;">
                    <h1 style="color: white; margin: 0; font-size: 28px;">🍽️ Welcome to Foodiya!</h1>
                </div>
                <div style="background: #ffffff; padding: 30px; border: 1px solid #e0e0e0; border-radius: 0 0 12px 12px;">
                    <h2 style="color: #333;">Hello, {firstName}! 👋</h2>
                    <p style="color: #555; font-size: 16px; line-height: 1.6;">
                        We're thrilled to have you join our community of food lovers and home chefs!
                        Your username is <strong>@{userName}</strong>.
                    </p>
                    <p style="color: #555; font-size: 16px; line-height: 1.6;">Here's what you can do next:</p>
                    <ul style="color: #555; font-size: 15px; line-height: 1.8;">
                        <li>🧑‍🍳 Set up your <strong>Chef Profile</strong></li>
                        <li>📖 Browse recipes from our community</li>
                        <li>🍳 Share your own recipes with the world</li>
                        <li>❤️ Like and save your favorite dishes</li>
                    </ul>
                    <div style="text-align: center; margin-top: 25px;">
                        <a href="https://foodiya.app" style="background: #ff6b35; color: white; padding: 14px 32px; text-decoration: none; border-radius: 8px; font-size: 16px; font-weight: bold;">
                            Start Exploring Recipes
                        </a>
                    </div>
                    <p style="color: #999; font-size: 13px; margin-top: 30px; text-align: center;">
                        Happy cooking! 🥘<br/>The Foodiya Team
                    </p>
                </div>
            </body>
            </html>
            """;
    }
}
