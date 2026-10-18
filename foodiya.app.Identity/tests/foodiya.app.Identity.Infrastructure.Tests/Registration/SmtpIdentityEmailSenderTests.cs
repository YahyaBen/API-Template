using Microsoft.Extensions.Logging.Abstractions;
using foodiya.app.Identity.Application.Exceptions;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Domain.Enums;
using foodiya.app.Identity.Infrastructure.Registration;

namespace foodiya.app.Identity.Infrastructure.Tests.Registration;

[TestClass]
public sealed class SmtpIdentityEmailSenderTests
{
    private readonly TestContext _testContext;

    public SmtpIdentityEmailSenderTests(TestContext testContext) => _testContext = testContext;

    [TestMethod]
    public async Task SendAccountOtpAsync_PasswordReset_IncludesCodeChallengeAndExpiry()
    {
        var transport = new CapturingTransport();
        var sender = CreateSender(transport);
        var challengeId = Guid.Parse("123e4567-e89b-12d3-a456-426614174000");
        var expiresAt = new DateTimeOffset(2026, 9, 7, 10, 0, 0, TimeSpan.Zero);

        await sender.SendAccountOtpAsync(
            "person@example.test",
            new IdentityEmailContext("Person", "en"),
            OtpPurpose.PasswordReset,
            challengeId, "123456", expiresAt, _testContext.CancellationToken);

        Assert.IsNotNull(transport.Message);
        Assert.AreEqual("person@example.test", transport.Message.Recipient);
        Assert.AreEqual("Reset your foodiya.app.Identity password", transport.Message.Subject);
        Assert.Contains("123456", transport.Message.TextBody);
        Assert.Contains(challengeId.ToString("D"), transport.Message.TextBody);
        Assert.Contains(expiresAt.ToString("O"), transport.Message.TextBody);
        Assert.Contains("foodiya.app.Identity", transport.Message.HtmlBody);
        Assert.Contains("123456", transport.Message.HtmlBody);
        Assert.DoesNotContain("<img", transport.Message.HtmlBody);
        Assert.DoesNotContain("{{", transport.Message.HtmlBody);
    }

    [TestMethod]
    public async Task SendRegistrationOtpAsync_BuildsConfirmationMessage()
    {
        var transport = new CapturingTransport();
        var sender = CreateSender(transport);

        var challengeId = Guid.Parse("223e4567-e89b-12d3-a456-426614174000");
        await sender.SendRegistrationOtpAsync(
            "person@example.test",
            new IdentityEmailContext("Person", "en"),
            challengeId,
            "654321",
            new DateTimeOffset(2026, 9, 7, 10, 0, 0, TimeSpan.Zero), _testContext.CancellationToken);

        Assert.IsNotNull(transport.Message);
        Assert.AreEqual("Confirm your foodiya.app.Identity account", transport.Message.Subject);
        Assert.Contains("654321", transport.Message.TextBody);
        Assert.Contains(challengeId.ToString("D"), transport.Message.HtmlBody);
        Assert.Contains("Confirm your account", transport.Message.HtmlBody);
    }

    [TestMethod]
    public async Task SendAccountOtpAsync_AccountDeletion_UsesDeletionTemplate()
    {
        var transport = new CapturingTransport();
        var sender = CreateSender(transport);

        await sender.SendAccountOtpAsync(
            "person@example.test",
            new IdentityEmailContext("Person", "en"),
            OtpPurpose.AccountDeletion,
            Guid.Parse("323e4567-e89b-12d3-a456-426614174000"),
            "112233",
            new DateTimeOffset(2026, 9, 7, 10, 0, 0, TimeSpan.Zero),
            _testContext.CancellationToken);

        Assert.IsNotNull(transport.Message);
        Assert.AreEqual("Confirm deletion of your foodiya.app.Identity account", transport.Message.Subject);
        Assert.Contains("Deletion confirmation code", transport.Message.HtmlBody);
        Assert.Contains("112233", transport.Message.HtmlBody);
        Assert.DoesNotContain("<img", transport.Message.HtmlBody);
    }

    [TestMethod]
    public async Task SendAccountOtpAsync_DeviceLogin_UsesDeviceDetailsAndEscapesValues()
    {
        var transport = new CapturingTransport();
        var sender = CreateSender(transport);

        await sender.SendAccountOtpAsync(
            "person@example.test",
            new IdentityEmailContext("<script>Person</script>", "en", "Kitchen tablet", "2.4.0",
                new DateTimeOffset(2026, 9, 7, 9, 30, 0, TimeSpan.Zero)),
            OtpPurpose.DeviceLogin,
            Guid.Parse("423e4567-e89b-12d3-a456-426614174000"),
            "445566",
            new DateTimeOffset(2026, 9, 7, 10, 0, 0, TimeSpan.Zero),
            _testContext.CancellationToken);

        Assert.IsNotNull(transport.Message);
        Assert.Contains("Kitchen tablet", transport.Message.HtmlBody);
        Assert.Contains("2.4.0", transport.Message.HtmlBody);
        Assert.Contains("&lt;script&gt;Person&lt;/script&gt;", transport.Message.HtmlBody);
        Assert.DoesNotContain("<script>Person</script>", transport.Message.HtmlBody);
    }

    [TestMethod]
    public async Task SendWelcomeAndDeletionConfirmationAsync_UseBrandedLogoFreeTemplates()
    {
        var transport = new CapturingTransport();
        var sender = CreateSender(transport);
        var context = new IdentityEmailContext("Person", "en");

        await sender.SendWelcomeAsync("person@example.test", context, _testContext.CancellationToken);
        Assert.IsNotNull(transport.Message);
        Assert.AreEqual("Welcome to foodiya.app.Identity", transport.Message.Subject);
        Assert.Contains("your account is ready", transport.Message.HtmlBody);
        Assert.DoesNotContain("<img", transport.Message.HtmlBody);

        await sender.SendAccountDeletionConfirmationAsync(
            "person@example.test", context, _testContext.CancellationToken);
        Assert.IsNotNull(transport.Message);
        Assert.AreEqual("Your foodiya.app.Identity account was deleted", transport.Message.Subject);
        Assert.Contains("deletion of your foodiya.app.Identity account is complete", transport.Message.HtmlBody);
        Assert.DoesNotContain("<img", transport.Message.HtmlBody);
    }

    [TestMethod]
    public async Task AllEmailTypes_WithArabicPreference_UseArabicAndRtlHtml()
    {
        var transport = new CapturingTransport();
        var sender = CreateSender(transport);
        var context = new IdentityEmailContext(
            "ليلى",
            "ar-MA",
            "هاتف ليلى",
            "2.4.0",
            new DateTimeOffset(2026, 9, 7, 9, 30, 0, TimeSpan.Zero));
        var challengeId = Guid.Parse("623e4567-e89b-12d3-a456-426614174000");
        var expiresAt = new DateTimeOffset(2026, 9, 7, 10, 0, 0, TimeSpan.Zero);

        await sender.SendRegistrationOtpAsync(
            "person@example.test", context, challengeId, "123456", expiresAt,
            _testContext.CancellationToken);
        Assert.IsNotNull(transport.Message);
        Assert.AreEqual("أكّد حسابك في foodiya.app.Identity", transport.Message.Subject);
        Assert.Contains("رمز التحقق", transport.Message.TextBody);
        AssertArabicHtml(transport.Message.HtmlBody, "تأكيد حسابك");

        await sender.SendAccountOtpAsync(
            "person@example.test", context, OtpPurpose.PasswordReset, challengeId, "123456", expiresAt,
            _testContext.CancellationToken);
        Assert.IsNotNull(transport.Message);
        Assert.AreEqual("إعادة تعيين كلمة مرور foodiya.app.Identity", transport.Message.Subject);
        AssertArabicHtml(transport.Message.HtmlBody, "إعادة تعيين كلمة المرور");

        await sender.SendAccountOtpAsync(
            "person@example.test", context, OtpPurpose.AccountDeletion, challengeId, "123456", expiresAt,
            _testContext.CancellationToken);
        Assert.IsNotNull(transport.Message);
        Assert.AreEqual("تأكيد حذف حسابك في foodiya.app.Identity", transport.Message.Subject);
        AssertArabicHtml(transport.Message.HtmlBody, "رمز تأكيد الحذف");

        await sender.SendAccountOtpAsync(
            "person@example.test", context, OtpPurpose.DeviceLogin, challengeId, "123456", expiresAt,
            _testContext.CancellationToken);
        Assert.IsNotNull(transport.Message);
        Assert.AreEqual("التحقق من جهاز مرتبط بحسابك في foodiya.app.Identity", transport.Message.Subject);
        AssertArabicHtml(transport.Message.HtmlBody, "هاتف ليلى");

        await sender.SendWelcomeAsync(
            "person@example.test", context, _testContext.CancellationToken);
        Assert.IsNotNull(transport.Message);
        Assert.AreEqual("مرحبًا بك في foodiya.app.Identity", transport.Message.Subject);
        AssertArabicHtml(transport.Message.HtmlBody, "حسابك جاهز");

        await sender.SendAccountDeletionConfirmationAsync(
            "person@example.test", context, _testContext.CancellationToken);
        Assert.IsNotNull(transport.Message);
        Assert.AreEqual("تم حذف حسابك في foodiya.app.Identity", transport.Message.Subject);
        AssertArabicHtml(transport.Message.HtmlBody, "اكتملت عملية حذف حسابك");
    }

    [TestMethod]
    public async Task SendRegistrationOtpAsync_WhenTransportFails_ThrowsControlledDeliveryError()
    {
        var sender = CreateSender(new FailingTransport());

        var exception = await Assert.ThrowsExactlyAsync<IdentityDeliveryException>(() =>
            sender.SendRegistrationOtpAsync(
                "person@example.test",
                new IdentityEmailContext("Person", "en"),
                Guid.Parse("523e4567-e89b-12d3-a456-426614174000"),
                "654321",
                new DateTimeOffset(2026, 9, 7, 10, 0, 0, TimeSpan.Zero),
                _testContext.CancellationToken));

        Assert.AreEqual("EMAIL_DELIVERY_FAILED", exception.Code);
    }

    private static SmtpIdentityEmailSender CreateSender(IIdentityEmailTransport transport) =>
        new(transport, NullLogger<SmtpIdentityEmailSender>.Instance);

    private static void AssertArabicHtml(string html, string expectedText)
    {
        Assert.Contains("lang=\"ar\" dir=\"rtl\"", html);
        Assert.Contains("direction:rtl", html);
        Assert.Contains(expectedText, html);
        Assert.DoesNotContain("{{", html);
        Assert.DoesNotContain("<img", html);
    }

    private sealed class CapturingTransport : IIdentityEmailTransport
    {
        public IdentityEmailMessage? Message { get; private set; }

        public Task SendAsync(IdentityEmailMessage message, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Message = message;
            return Task.CompletedTask;
        }
    }

    private sealed class FailingTransport : IIdentityEmailTransport
    {
        public Task SendAsync(IdentityEmailMessage message, CancellationToken cancellationToken) =>
            throw new InvalidOperationException("Simulated SMTP failure.");
    }
}
