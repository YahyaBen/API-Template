namespace Foodiya.Domain.Configuration;

public sealed class GoogleAuthOptions
{
    public const string SectionName = "Authentication:Google";

    public string[] ClientIds { get; set; } = [];
}
