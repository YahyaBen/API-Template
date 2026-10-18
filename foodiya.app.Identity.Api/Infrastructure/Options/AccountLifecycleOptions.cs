namespace foodiya.app.Identity.Infrastructure.Options;

public sealed class AccountLifecycleOptions
{
    public const string SectionName = "AccountLifecycle";
    public int PasswordHistoryCount { get; init; } = 5;
}
