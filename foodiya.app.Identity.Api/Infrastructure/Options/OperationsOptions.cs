namespace foodiya.app.Identity.Infrastructure.Options;

public sealed class CleanupOptions
{
    public const string SectionName = "Cleanup";

    public bool Enabled { get; init; }
    public int IntervalMinutes { get; init; } = 60;
    public int BatchSize { get; init; } = 500;
    public int SessionRetentionDays { get; init; } = 30;
    public int OtpRetentionDays { get; init; } = 7;
    public int ProcessedEventRetentionDays { get; init; } = 30;
    public int RegistrationAttemptRetentionDays { get; init; } = 30;
    public int DeletedAccountRetentionDays { get; init; } = 90;
}

public sealed class OutboxOptions
{
    public const string SectionName = "Outbox";

    public bool Enabled { get; init; }
    public string Transport { get; init; } = "Webhook";
    public string Endpoint { get; init; } = string.Empty;
    public string SigningSecret { get; init; } = string.Empty;
    public int IntervalSeconds { get; init; } = 5;
    public int BatchSize { get; init; } = 50;
    public int MaxAttempts { get; init; } = 10;
    public int LeaseSeconds { get; init; } = 120;
    public int TimeoutSeconds { get; init; } = 20;
}

public sealed class EmailDeliveryOptions
{
    public const string SectionName = "EmailDelivery";

    public string Provider { get; init; } = "Disabled";
    public string Host { get; init; } = string.Empty;
    public int Port { get; init; } = 587;
    public string FromAddress { get; init; } = string.Empty;
    public string UserName { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public int TimeoutSeconds { get; init; } = 20;
    public bool LogDevelopmentCodes { get; init; }
    public bool UseStartTls { get; init; } = true;
}
