using Microsoft.AspNetCore.Http.HttpResults;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Contracts.Authentication;

namespace foodiya.app.Identity.Api.Endpoints;

internal static class AuthenticationEndpoints
{
    public static IEndpointRouteBuilder MapAuthenticationEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth").RequireRateLimiting("identity-sensitive")
            .WithTags("Authentication");

        group.MapPost("/login", LoginAsync)
            .WithName("LoginIdentity")
            .WithSummary("Authenticate an active account and create a device session.")
            .WithDescription("Accepts an email address or user name and returns a short-lived JWT plus an opaque refresh token.")
            .Accepts<LoginRequest>("application/json")
            .Produces<AuthenticationTokenResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status423Locked);

        group.MapPost("/google", GoogleAsync)
            .WithName("GoogleLoginIdentity")
            .WithSummary("Authenticate with a Google ID token and create a device session.")
            .WithDescription("Validates the Google ID token for a configured client, links or creates an active account, and returns the standard token pair.")
            .Accepts<GoogleSignInRequest>("application/json")
            .Produces<AuthenticationTokenResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status423Locked);

        group.MapPost("/token/refresh", RefreshAsync)
            .WithName("RefreshIdentityToken")
            .WithSummary("Rotate a refresh token and issue a new token pair.")
            .WithDescription("A reused token revokes its entire session to contain credential theft.")
            .Accepts<RefreshAccessTokenRequest>("application/json")
            .Produces<AuthenticationTokenResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status423Locked);

        return endpoints;
    }

    private static async Task<Ok<AuthenticationTokenResponse>> LoginAsync(
        LoginRequest request,
        HttpContext httpContext,
        IIdentityAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        var response = await authenticationService.LoginAsync(
            request,
            httpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);
        return TypedResults.Ok(response);
    }

    private static async Task<Ok<AuthenticationTokenResponse>> RefreshAsync(
        RefreshAccessTokenRequest request,
        HttpContext httpContext,
        IIdentityAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        var response = await authenticationService.RefreshAsync(
            request,
            httpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);
        return TypedResults.Ok(response);
    }

    private static async Task<Ok<AuthenticationTokenResponse>> GoogleAsync(
        GoogleSignInRequest request,
        HttpContext httpContext,
        IIdentityAuthenticationService authenticationService,
        CancellationToken cancellationToken)
    {
        var response = await authenticationService.SignInWithGoogleAsync(
            request,
            httpContext.Connection.RemoteIpAddress?.ToString(),
            cancellationToken);
        return TypedResults.Ok(response);
    }
}
