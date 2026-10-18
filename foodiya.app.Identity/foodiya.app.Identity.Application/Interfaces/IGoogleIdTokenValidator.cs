namespace foodiya.app.Identity.Application.Interfaces;

public interface IGoogleIdTokenValidator
{
    Task<GoogleIdentity> ValidateAsync(
        string idToken,
        CancellationToken cancellationToken);
}

public sealed record GoogleIdentity(
    string Subject,
    string Email,
    bool EmailVerified,
    string? GivenName,
    string? FamilyName,
    string? HostedDomain);
