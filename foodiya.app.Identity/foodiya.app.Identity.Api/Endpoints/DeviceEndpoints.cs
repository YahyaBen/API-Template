using Microsoft.AspNetCore.Http.HttpResults;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Contracts.Accounts;
using foodiya.app.Identity.Contracts.Devices;

namespace foodiya.app.Identity.Api.Endpoints;

internal static class DeviceEndpoints
{
    public static IEndpointRouteBuilder MapDeviceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/users/me").WithTags("Devices and sessions").RequireAuthorization();
        Describe(group.MapGet("/devices", GetDevicesAsync), "ListIdentityDevices", "List account devices with bounded pagination.");
        Describe(group.MapGet("/sessions", GetSessionsAsync), "ListIdentitySessions", "List account sessions with bounded pagination.");
        Describe(group.MapPost("/devices/{deviceId:guid}/revoke", RevokeDeviceAsync), "RevokeIdentityDevice", "Revoke a device and all its sessions.")
            .ProducesProblem(StatusCodes.Status404NotFound);
        Describe(group.MapPost("/sessions/{sessionId:guid}/revoke", RevokeSessionAsync), "RevokeIdentitySession", "Revoke one session and its refresh tokens.")
            .ProducesProblem(StatusCodes.Status404NotFound);
        Describe(group.MapPut("/devices/current/push-registration", SetPushAsync), "SetIdentityPushRegistration", "Register the current device's Firebase installation for push notifications.")
            .ProducesProblem(StatusCodes.Status409Conflict);
        Describe(group.MapDelete("/devices/current/push-registration", ClearPushAsync), "ClearIdentityPushRegistration", "Clear the current device's Firebase installation.");
        Describe(group.MapPost("/devices/current/verification/request", RequestVerificationAsync), "RequestIdentityDeviceVerification", "Email a verification code bound to the current device.")
            .ProducesProblem(StatusCodes.Status409Conflict).ProducesProblem(StatusCodes.Status429TooManyRequests)
            .ProducesProblem(StatusCodes.Status503ServiceUnavailable);
        Describe(group.MapPost("/devices/current/verification/confirm", ConfirmVerificationAsync), "ConfirmIdentityDeviceVerification", "Confirm email verification for the current device.")
            .WithDescription("Records device trust without changing password-login requirements. The OTP is bound to this account and installation.");
        return endpoints;
    }

    private static RouteHandlerBuilder Describe(RouteHandlerBuilder route, string name, string summary) =>
        route.WithName(name).WithSummary(summary).ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status400BadRequest);

    private static async Task<Ok<DeviceListResponse>> GetDevicesAsync(HttpContext context, IDeviceSessionService service,
        CancellationToken cancellationToken, int skip = 0, int take = 25) =>
        TypedResults.Ok(await service.GetDevicesAsync(context.User, skip, take, cancellationToken));

    private static async Task<Ok<SessionListResponse>> GetSessionsAsync(HttpContext context, IDeviceSessionService service,
        CancellationToken cancellationToken, int skip = 0, int take = 25) =>
        TypedResults.Ok(await service.GetSessionsAsync(context.User, skip, take, cancellationToken));

    private static async Task<NoContent> RevokeDeviceAsync(Guid deviceId, HttpContext context, IDeviceSessionService service, CancellationToken cancellationToken)
    {
        await service.RevokeDeviceAsync(context.User, deviceId, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> RevokeSessionAsync(Guid sessionId, HttpContext context, IDeviceSessionService service, CancellationToken cancellationToken)
    {
        await service.RevokeSessionAsync(context.User, sessionId, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> SetPushAsync(UpdateFirebaseInstallationRequest request, HttpContext context, IDeviceSessionService service, CancellationToken cancellationToken)
    {
        await service.SetFirebaseInstallationAsync(context.User, request.FirebaseInstallationId, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<NoContent> ClearPushAsync(HttpContext context, IDeviceSessionService service, CancellationToken cancellationToken)
    {
        await service.SetFirebaseInstallationAsync(context.User, null, cancellationToken);
        return TypedResults.NoContent();
    }

    private static async Task<Ok<AccountChallengeResponse>> RequestVerificationAsync(HttpContext context, IDeviceSessionService service, CancellationToken cancellationToken) =>
        TypedResults.Ok(await service.RequestVerificationAsync(context.User, cancellationToken));

    private static async Task<NoContent> ConfirmVerificationAsync(ConfirmDeviceVerificationRequest request, HttpContext context, IDeviceSessionService service, CancellationToken cancellationToken)
    {
        await service.ConfirmVerificationAsync(context.User, request, cancellationToken);
        return TypedResults.NoContent();
    }
}
