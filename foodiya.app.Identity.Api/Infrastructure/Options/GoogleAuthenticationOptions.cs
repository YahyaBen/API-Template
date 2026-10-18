namespace foodiya.app.Identity.Infrastructure.Options;

public sealed class GoogleAuthenticationOptions
{
    public const string SectionName = "Authentication:Google";

    public string[] ClientIds { get; set; } = [];
}
