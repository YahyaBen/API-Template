using System.Data;
using System.Text.Json;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using foodiya.app.Identity.Domain.Constants;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Domain.Enums;
using foodiya.app.Identity.Infrastructure.Identity;
using foodiya.app.Identity.Infrastructure.Options;
using foodiya.app.Identity.Infrastructure.Persistence;

namespace foodiya.app.Identity.Infrastructure.Authorization;

internal sealed class SuperAdminBootstrapHostedService(
    IServiceScopeFactory scopeFactory,
    IOptions<BootstrapAdminOptions> options,
    TimeProvider timeProvider,
    ILogger<SuperAdminBootstrapHostedService> logger) : IHostedService
{
    private readonly BootstrapAdminOptions _options = options.Value;

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        if (!_options.Enabled)
        {
            return;
        }

        await using var scope = scopeFactory.CreateAsyncScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var dataContext = scope.ServiceProvider.GetRequiredService<IdentityDataContext>();
        await using var transaction = await dataContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();
        if ((await userManager.GetUsersInRoleAsync(RoleNames.SuperAdmin)).Count > 0)
        {
            await transaction.CommitAsync(cancellationToken);
            logger.LogInformation("Super-administrator bootstrap skipped because one already exists.");
            return;
        }

        var email = _options.Email.Trim();
        if (await userManager.FindByEmailAsync(email) is not null)
        {
            throw new InvalidOperationException(
                "BootstrapAdmin:Email already belongs to an account. Disable bootstrap or use a new address.");
        }

        var now = timeProvider.GetUtcNow();
        var user = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = _options.UserName.Trim(),
            Email = email,
            FirstName = _options.FirstName.Trim(),
            LastName = _options.LastName.Trim(),
            EmailConfirmed = true,
            Status = AccountStatus.Active,
            CreatedAtUtc = now,
            LockoutEnabled = true,
            SecurityStamp = Guid.NewGuid().ToString("N")
        };

        EnsureSucceeded(
            await userManager.CreateAsync(user, _options.Password),
            "The bootstrap super administrator could not be created.");
        EnsureSucceeded(
            await userManager.AddToRolesAsync(user, [RoleNames.User, RoleNames.SuperAdmin]),
            "The bootstrap roles could not be assigned.");

        dataContext.OutboxMessages.Add(new OutboxMessage
        {
            OccurredAtUtc = now,
            Type = "SuperAdministratorBootstrapped",
            Payload = JsonSerializer.Serialize(new { user.Id, user.Email })
        });
        await dataContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        logger.LogWarning(
            "Bootstrap super administrator {UserId} was created. Disable BootstrapAdmin and remove its password from configuration now.",
            user.Id);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

    private static void EnsureSucceeded(IdentityResult result, string message)
    {
        if (result.Succeeded)
        {
            return;
        }

        var details = string.Join(" ", result.Errors.Select(error => error.Description));
        throw new InvalidOperationException($"{message} {details}");
    }
}
