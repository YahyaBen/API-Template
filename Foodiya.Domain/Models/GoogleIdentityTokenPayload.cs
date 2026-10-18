namespace Foodiya.Domain.Models;

public sealed class GoogleIdentityTokenPayload
{
    public string Subject { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public bool EmailVerified { get; init; }
    public string? GivenName { get; init; }
    public string? FamilyName { get; init; }
    public string? FullName { get; init; }
    public string? PictureUrl { get; init; }
    public string? HostedDomain { get; init; }
}
