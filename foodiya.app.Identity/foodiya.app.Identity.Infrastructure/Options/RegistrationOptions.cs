namespace foodiya.app.Identity.Infrastructure.Options;

public sealed class RegistrationOptions
{
    public const string SectionName = "Registration";

    public string OtpPepper { get; init; } = string.Empty;
    public int OtpLifetimeMinutes { get; init; } = 10;
    public int OtpMaxAttempts { get; init; } = 5;
    public int OtpResendCooldownSeconds { get; init; } = 60;
}
