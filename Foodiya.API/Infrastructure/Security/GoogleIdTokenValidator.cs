using Foodiya.Domain.Configuration;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Auth;
using Foodiya.Domain.Models;
using Google.Apis.Auth;
using Microsoft.Extensions.Options;

namespace Foodiya.Infrastructure.Security;

public sealed class GoogleIdTokenValidator : IGoogleIdTokenValidator
{
    private readonly GoogleAuthOptions _options;

    public GoogleIdTokenValidator(IOptions<GoogleAuthOptions> options)
    {
        _options = options.Value;
    }

    public async Task<GoogleIdentityTokenPayload> ValidateAsync(string idToken, CancellationToken ct = default)
    {
        if (_options.ClientIds.Length == 0)
            throw new FoodiyaBadRequestException("Google sign-in is not configured on this API.");

        if (string.IsNullOrWhiteSpace(idToken))
            throw new FoodiyaUnauthorizedException("Google ID token is required.");

        try
        {
            var payload = await GoogleJsonWebSignature.ValidateAsync(
                idToken,
                new GoogleJsonWebSignature.ValidationSettings
                {
                    Audience = _options.ClientIds
                });

            if (string.IsNullOrWhiteSpace(payload.Subject))
                throw new FoodiyaUnauthorizedException("Google token subject is missing.");

            if (string.IsNullOrWhiteSpace(payload.Email))
                throw new FoodiyaUnauthorizedException("Google token email is missing.");

            return new GoogleIdentityTokenPayload
            {
                Subject = payload.Subject,
                Email = payload.Email,
                EmailVerified = payload.EmailVerified,
                GivenName = payload.GivenName,
                FamilyName = payload.FamilyName,
                FullName = payload.Name,
                PictureUrl = payload.Picture,
                HostedDomain = payload.HostedDomain
            };
        }
        catch (InvalidJwtException ex)
        {
            throw new FoodiyaUnauthorizedException("Invalid Google ID token.", ex);
        }
    }
}
