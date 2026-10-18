namespace foodiya.app.Identity.Infrastructure.Options;

public sealed class AuthenticationTokenOptions
{
    public const string SectionName = "Tokens";

    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string KeyId { get; init; } = "foodiya-app-identity-v1";
    public string? RsaPrivateKeyPem { get; init; }
    public string? RsaPrivateKeyPath { get; init; }
    public string RefreshTokenPepper { get; init; } = string.Empty;
    public int AccessTokenLifetimeMinutes { get; init; } = 15;
    public int RefreshTokenLifetimeDays { get; init; } = 30;
}
