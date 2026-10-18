using Microsoft.AspNetCore.Http.HttpResults;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Contracts.Users;

namespace foodiya.app.Identity.Api.Endpoints;

internal static class UserEndpoints
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/users/me", GetCurrentIdentityAsync)
            .WithName("GetCurrentIdentity")
            .WithTags("Users")
            .WithSummary("Get the authenticated account and current session.")
            .WithDescription("Returns fresh account, role, session, and device data from identity storage.")
            .Produces<CurrentIdentityResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .RequireAuthorization();

        return endpoints;
    }

    private static async Task<Ok<CurrentIdentityResponse>> GetCurrentIdentityAsync(
        HttpContext httpContext,
        ICurrentIdentityService currentIdentityService,
        CancellationToken cancellationToken)
    {
        var response = await currentIdentityService.GetAsync(
            httpContext.User,
            cancellationToken);
        return TypedResults.Ok(response);
    }
}
