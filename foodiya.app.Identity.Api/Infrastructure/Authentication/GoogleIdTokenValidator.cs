using Google.Apis.Auth;
using Microsoft.Extensions.Options;
using foodiya.app.Identity.Application.Exceptions;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Infrastructure.Options;

namespace foodiya.app.Identity.Infrastructure.Authentication;

internal sealed class GoogleIdTokenValidator(
    IOptions<GoogleAuthenticationOptions> options) : IGoogleIdTokenValidator
{
    private readonly GoogleAuthenticationOptions _options = options.Value;

    public async Task<GoogleIdentity> ValidateAsync(
        string idToken,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_options.ClientIds.Length == 0)
        {
            throw new IdentityValidationException(
                "GOOGLE_AUTH_NOT_CONFIGURED",
                "Google authentication is not configured.");
        }

        if (string.IsNullOrWhiteSpace(idToken))
        {
            throw InvalidToken();
        }

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = _options.ClientIds
                });
            cancellationToken.ThrowIfCancellationRequested();

            if (string.IsNullOrWhiteSpace(payload.Subject)
                || string.IsNullOrWhiteSpace(payload.Email) || payload.Email.Length > 256)
            {
                throw InvalidToken();
            }

            return new GoogleIdentity(
                payload.Subject,
                payload.Email,
                payload.EmailVerified,
                payload.GivenName,
                payload.FamilyName,
                payload.HostedDomain);
        }
        catch (InvalidJwtException)
        {
            throw InvalidToken();
        }
    }

    private static IdentityUnauthorizedException InvalidToken() => new(
        "GOOGLE_ID_TOKEN_INVALID",
        "The Google ID token is invalid or expired.");
}
