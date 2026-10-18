using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Options;
using foodiya.app.Identity.Infrastructure.Authentication;
using foodiya.app.Identity.Infrastructure.Options;

namespace foodiya.app.Identity.Api.Endpoints;

internal static class OidcDiscoveryEndpoints
{
    public static IEndpointRouteBuilder MapOidcDiscoveryEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/.well-known/openid-configuration", GetOpenIdConfiguration)
            .WithName("GetOpenIdConfiguration")
            .WithSummary("OpenID Connect discovery document.")
            .WithDescription("Provides metadata about this identity provider, including the JWKS URI and supported algorithms.")
            .WithTags("Discovery")
            .AllowAnonymous();

        endpoints.MapGet("/.well-known/jwks.json", GetJwks)
            .WithName("GetJwks")
            .WithSummary("JSON Web Key Set (JWKS).")
            .WithDescription("Provides public keys used by resource APIs to verify JWT signatures.")
            .WithTags("Discovery")
            .AllowAnonymous();

        return endpoints;
    }

    private static Ok<OpenIdConfigurationResponse> GetOpenIdConfiguration(
        IOptions<AuthenticationTokenOptions> options)
    {
        var issuer = options.Value.Issuer.TrimEnd('/');
        return TypedResults.Ok(new OpenIdConfigurationResponse(
            issuer,
            $"{issuer}/.well-known/jwks.json"));
    }

    private static IResult GetJwks(IRsaSigningKeyProvider keyProvider)
    {
        return Results.Json(keyProvider.GetPublicJwks(), contentType: "application/json");
    }
}

internal sealed record OpenIdConfigurationResponse(
    [property: JsonPropertyName("issuer")] string Issuer,
    [property: JsonPropertyName("jwks_uri")] string JwksUri);
