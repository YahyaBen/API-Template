namespace Foodiya.Infrastructure.Messaging;

public sealed class IdentityEventsOptions
{
    public const string SectionName = "IdentityEvents";
    public bool Enabled { get; init; }
    public string Queue { get; init; } = "foodiya.profiles.identity";
    public int MaxAttempts { get; init; } = 5;
    public int RetryDelaySeconds { get; init; } = 5;
}
