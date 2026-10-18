using foodiya.app.Identity.Contracts.Roles;

namespace foodiya.app.Identity.Application.Interfaces;

public interface IRoleManagementService
{
    RoleCatalogResponse GetAvailableRoles();

    Task<UserRolesResponse> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken);

    Task<UserRolesResponse> ReplaceUserRolesAsync(
        Guid actorUserId,
        Guid userId,
        ReplaceUserRolesRequest request,
        CancellationToken cancellationToken);
}
