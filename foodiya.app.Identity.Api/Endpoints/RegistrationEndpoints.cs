using Microsoft.AspNetCore.Http.HttpResults;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Contracts.Registration;

namespace foodiya.app.Identity.Api.Endpoints;

internal static class RegistrationEndpoints
{
    public static IEndpointRouteBuilder MapRegistrationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth").RequireRateLimiting("identity-sensitive")
            .WithTags("Registration");

        group.MapPost("/register", RegisterAsync)
            .WithName("RegisterIdentity")
            .WithSummary("Create a pending account and send an email confirmation code.")
            .Accepts<RegisterRequest>("application/json")
            .Produces<RegistrationChallengeResponse>(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        group.MapPost("/register/confirm", ConfirmAsync)
            .WithName("ConfirmIdentityRegistration")
            .WithSummary("Confirm a pending account using its email OTP challenge.")
            .Accepts<ConfirmRegistrationRequest>("application/json")
            .Produces<RegistrationConfirmationResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status409Conflict);

        group.MapPost("/register/resend", ResendAsync)
            .WithName("ResendIdentityRegistrationOtp")
            .WithSummary("Invalidate the previous code and send a replacement after the cooldown.")
            .Accepts<ResendRegistrationOtpRequest>("application/json")
            .Produces<RegistrationChallengeResponse>(StatusCodes.Status202Accepted)
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);

        return endpoints;
    }

    private static async Task<Accepted<RegistrationChallengeResponse>> RegisterAsync(
        RegisterRequest request,
        IRegistrationService registrationService,
        CancellationToken cancellationToken)
    {
        var response = await registrationService.RegisterAsync(request, cancellationToken);
        return TypedResults.Accepted("/api/auth/register/confirm", response);
    }

    private static async Task<Ok<RegistrationConfirmationResponse>> ConfirmAsync(
        ConfirmRegistrationRequest request,
        IRegistrationService registrationService,
        CancellationToken cancellationToken)
    {
        var response = await registrationService.ConfirmAsync(request, cancellationToken);
        return TypedResults.Ok(response);
    }

    private static async Task<Accepted<RegistrationChallengeResponse>> ResendAsync(
        ResendRegistrationOtpRequest request,
        IRegistrationService registrationService,
        CancellationToken cancellationToken)
    {
        var response = await registrationService.ResendAsync(request, cancellationToken);
        return TypedResults.Accepted("/api/auth/register/confirm", response);
    }
}
