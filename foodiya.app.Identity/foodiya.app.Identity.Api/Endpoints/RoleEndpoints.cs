using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Http.HttpResults;
using foodiya.app.Identity.Application.Exceptions;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Contracts.Roles;
using foodiya.app.Identity.Domain.Constants;

namespace foodiya.app.Identity.Api.Endpoints;

internal static class RoleEndpoints
{
    public static IEndpointRouteBuilder MapRoleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/admin")
            .WithTags("Roles");

        group.MapGet("/roles", GetAvailableRoles)
            .WithName("GetIdentityRoles")
            .WithSummary("List roles supported by this identity service.")
            .Produces<RoleCatalogResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .RequireAuthorization(AuthorizationPolicyNames.Admin);

        group.MapGet("/users/{userId:guid}/roles", GetUserRolesAsync)
            .WithName("GetIdentityUserRoles")
            .WithSummary("Get role assignments for an identity account.")
            .Produces<UserRolesResponse>()
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .RequireAuthorization(AuthorizationPolicyNames.Admin);

        group.MapPut("/users/{userId:guid}/roles", ReplaceUserRolesAsync)
            .WithName("ReplaceIdentityUserRoles")
            .WithSummary("Replace an account's roles and revoke its active sessions.")
            .WithDescription("Only a super administrator can change roles. The final super administrator cannot be demoted.")
            .Accepts<ReplaceUserRolesRequest>("application/json")
            .Produces<UserRolesResponse>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .ProducesProblem(StatusCodes.Status401Unauthorized)
            .ProducesProblem(StatusCodes.Status403Forbidden)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .RequireAuthorization(AuthorizationPolicyNames.SuperAdmin);

        return endpoints;
    }

    private static Ok<RoleCatalogResponse> GetAvailableRoles(
        IRoleManagementService roleManagementService) =>
        TypedResults.Ok(roleManagementService.GetAvailableRoles());

    private static async Task<Ok<UserRolesResponse>> GetUserRolesAsync(
        Guid userId,
        IRoleManagementService roleManagementService,
        CancellationToken cancellationToken)
    {
        var response = await roleManagementService.GetUserRolesAsync(
            userId,
            cancellationToken);
        return TypedResults.Ok(response);
    }

    private static async Task<Ok<UserRolesResponse>> ReplaceUserRolesAsync(
        Guid userId,
        ReplaceUserRolesRequest request,
        HttpContext httpContext,
        IRoleManagementService roleManagementService,
        CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(
            httpContext.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value,
            out var actorUserId))
        {
            throw new IdentityUnauthorizedException(
                "ACCESS_TOKEN_INVALID",
                "The access token does not identify a valid account.");
        }

        var response = await roleManagementService.ReplaceUserRolesAsync(
            actorUserId,
            userId,
            request,
            cancellationToken);
        return TypedResults.Ok(response);
    }
}
