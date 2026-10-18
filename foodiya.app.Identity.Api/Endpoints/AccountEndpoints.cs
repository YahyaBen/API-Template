using Microsoft.AspNetCore.Http.HttpResults;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Contracts.Accounts;

namespace foodiya.app.Identity.Api.Endpoints;

internal static class AccountEndpoints
{
    public static IEndpointRouteBuilder MapAccountEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var passwords = endpoints.MapGroup("/api/auth/password").WithTags("Passwords")
            .RequireRateLimiting("identity-sensitive");
        passwords.MapPost("/forgot", ForgotAsync)
            .WithName("RequestPasswordRecovery")
            .WithSummary("Email recovery instructions for an eligible account.")
            .WithDescription("Always returns the same acknowledgment, including during resend cooldown or email delivery failure.")
            .Produces<PasswordRecoveryResponse>(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status400BadRequest);
        passwords.MapPost("/reset", ResetAsync)
            .WithName("ResetIdentityPassword")
            .WithSummary("Reset a password using the challenge ID and OTP from email.")
            .WithDescription("Consumes the challenge, rejects recent passwords, and revokes all sessions.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest);
        passwords.MapPost("/change", ChangeAsync).RequireAuthorization()
            .WithName("ChangeIdentityPassword")
            .WithSummary("Change the current account's password and sign out all sessions.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status423Locked);

        var sessions = endpoints.MapGroup("/api/auth").WithTags("Sessions").RequireAuthorization();
        sessions.MapPost("/logout", LogoutAsync)
            .WithName("LogoutIdentitySession").WithSummary("Revoke the current session and its refresh tokens.")
            .Produces(StatusCodes.Status204NoContent).ProducesProblem(StatusCodes.Status401Unauthorized);
        sessions.MapPost("/logout-all", LogoutAllAsync)
            .WithName("LogoutAllIdentitySessions").WithSummary("Revoke every session belonging to the current account.")
            .Produces(StatusCodes.Status204NoContent).ProducesProblem(StatusCodes.Status401Unauthorized);

        var deletion = endpoints.MapGroup("/api/users/me/deletion").WithTags("Account deletion").RequireAuthorization();
        deletion.MapPost("/request", RequestDeletionAsync)
            .WithName("RequestIdentityDeletion")
            .WithSummary("Verify the password and send an email confirmation for account deletion.")
            .Produces<AccountChallengeResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest).ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict).ProducesProblem(StatusCodes.Status423Locked)
            .ProducesProblem(StatusCodes.Status429TooManyRequests).ProducesProblem(StatusCodes.Status503ServiceUnavailable);
        deletion.MapPost("/confirm", ConfirmDeletionAsync)
            .WithName("ConfirmIdentityDeletion")
            .WithSummary("Confirm deletion of the authenticated account with an email OTP.")
            .WithDescription("Removes credentials, anonymizes the account profile, revokes sessions and devices, and records a UserDeleted event. Historical event retention and downstream erasure are handled separately.")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status400BadRequest).ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status409Conflict);
        return endpoints;
    }

    private static async Task<Accepted<PasswordRecoveryResponse>> ForgotAsync(
        ForgotPasswordRequest request, IAccountLifecycleService service, CancellationToken cancellationToken) =>
        TypedResults.Accepted((string?)null, await service.ForgotPasswordAsync(request, cancellationToken));

    private static async Task<NoContent> ResetAsync(
        ResetPasswordRequest request, IAccountLifecycleService service, CancellationToken cancellationToken)
    {
        await service.ResetPasswordAsync(request, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> ChangeAsync(
        ChangePasswordRequest request, HttpContext context, IAccountLifecycleService service, CancellationToken cancellationToken)
    {
        await service.ChangePasswordAsync(context.User, request, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> LogoutAsync(
        HttpContext context, IAccountLifecycleService service, CancellationToken cancellationToken)
    {
        await service.LogoutAsync(context.User, false, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> LogoutAllAsync(
        HttpContext context, IAccountLifecycleService service, CancellationToken cancellationToken)
    {
        await service.LogoutAsync(context.User, true, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<AccountChallengeResponse>> RequestDeletionAsync(
        RequestAccountDeletionRequest request, HttpContext context, IAccountLifecycleService service, CancellationToken cancellationToken) =>
        TypedResults.Ok(await service.RequestDeletionAsync(context.User, request, cancellationToken));

    private static async Task<NoContent> ConfirmDeletionAsync(
        ConfirmAccountDeletionRequest request, HttpContext context, IAccountLifecycleService service, CancellationToken cancellationToken)
    {
        await service.ConfirmDeletionAsync(context.User, request, cancellationToken);
        return TypedResults.NoContent();
    }
}
