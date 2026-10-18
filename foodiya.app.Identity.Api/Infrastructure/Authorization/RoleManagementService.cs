using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using foodiya.app.Identity.Application.Exceptions;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Contracts.Roles;
using foodiya.app.Identity.Domain.Constants;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Infrastructure.Identity;
using foodiya.app.Identity.Infrastructure.Persistence;

namespace foodiya.app.Identity.Infrastructure.Authorization;

internal sealed class RoleManagementService(
    UserManager<ApplicationUser> userManager,
    IdentityDataContext dataContext,
    TimeProvider timeProvider) : IRoleManagementService
{
    public RoleCatalogResponse GetAvailableRoles() => new(RoleNames.All);

    public async Task<UserRolesResponse> GetUserRolesAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw UserNotFound();
        var roles = (await userManager.GetRolesAsync(user))
            .Order(StringComparer.Ordinal)
            .ToArray();
        return ToResponse(user, roles, user.UpdatedAtUtc ?? user.CreatedAtUtc);
    }

    public async Task<UserRolesResponse> ReplaceUserRolesAsync(
        Guid actorUserId,
        Guid userId,
        ReplaceUserRolesRequest request,
        CancellationToken cancellationToken)
    {
        var requestedRoles = request.Roles
            .Select(role => role.Trim().ToUpperInvariant())
            .Where(role => role.Length > 0)
            .Distinct(StringComparer.Ordinal)
            .Order(StringComparer.Ordinal)
            .ToArray();

        if (requestedRoles.Length == 0)
        {
            throw new IdentityValidationException(
                "ROLE_REQUIRED",
                "At least one role must be assigned.");
        }

        var invalidRoles = requestedRoles.Except(RoleNames.All, StringComparer.Ordinal).ToArray();
        if (invalidRoles.Length > 0)
        {
            throw new IdentityValidationException(
                "ROLE_INVALID",
                $"Unknown role: {string.Join(", ", invalidRoles)}.");
        }

        await using var transaction = await dataContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw UserNotFound();
        var existingRoles = (await userManager.GetRolesAsync(user))
            .Order(StringComparer.Ordinal)
            .ToArray();

        if (existingRoles.Contains(RoleNames.SuperAdmin, StringComparer.Ordinal)
            && !requestedRoles.Contains(RoleNames.SuperAdmin, StringComparer.Ordinal))
        {
            var superAdministrators = await userManager.GetUsersInRoleAsync(RoleNames.SuperAdmin);
            if (superAdministrators.Count <= 1)
            {
                throw new IdentityConflictException(
                    "LAST_SUPER_ADMIN_REQUIRED",
                    "The final super administrator cannot be demoted.");
            }
        }

        var rolesToRemove = existingRoles.Except(requestedRoles, StringComparer.Ordinal).ToArray();
        var rolesToAdd = requestedRoles.Except(existingRoles, StringComparer.Ordinal).ToArray();
        if (rolesToRemove.Length == 0 && rolesToAdd.Length == 0)
        {
            await transaction.CommitAsync(cancellationToken);
            return ToResponse(user, existingRoles, user.UpdatedAtUtc ?? user.CreatedAtUtc);
        }

        if (rolesToRemove.Length > 0)
        {
            EnsureSucceeded(await userManager.RemoveFromRolesAsync(user, rolesToRemove));
        }

        if (rolesToAdd.Length > 0)
        {
            EnsureSucceeded(await userManager.AddToRolesAsync(user, rolesToAdd));
        }

        var now = timeProvider.GetUtcNow();
        user.UpdatedAtUtc = now;
        EnsureSucceeded(await userManager.UpdateSecurityStampAsync(user));

        await dataContext.UserSessions
            .Where(session => session.UserId == user.Id && session.RevokedAtUtc == null)
            .ExecuteUpdateAsync(
                updates => updates
                    .SetProperty(session => session.RevokedAtUtc, now)
                    .SetProperty(session => session.RevocationReason, "RolesChanged"),
                cancellationToken);
        await dataContext.RefreshTokens
            .Where(token => token.UserId == user.Id && token.RevokedAtUtc == null)
            .ExecuteUpdateAsync(
                updates => updates
                    .SetProperty(token => token.RevokedAtUtc, now)
                    .SetProperty(token => token.RevocationReason, "RolesChanged"),
                cancellationToken);

        dataContext.OutboxMessages.Add(new OutboxMessage
        {
            OccurredAtUtc = now,
            Type = "UserRolesChanged",
            Payload = JsonSerializer.Serialize(new
            {
                UserId = user.Id,
                ActorUserId = actorUserId,
                Roles = requestedRoles
            })
        });
        await dataContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return ToResponse(user, requestedRoles, now);
    }

    private static UserRolesResponse ToResponse(
        ApplicationUser user,
        IReadOnlyCollection<string> roles,
        DateTimeOffset updatedAtUtc) => new(
            user.Id,
            user.Email ?? string.Empty,
            roles,
            updatedAtUtc);

    private static IdentityNotFoundException UserNotFound() => new(
        "USER_NOT_FOUND",
        "The requested identity account was not found.");

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (result.Succeeded)
        {
            return;
        }

        var message = string.Join(" ", result.Errors.Select(error => error.Description));
        throw new IdentityValidationException("ROLE_UPDATE_REJECTED", message);
    }
}
