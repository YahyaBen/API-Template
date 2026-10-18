using System.Data;
using System.Text.Json;
using foodiya.app.Identity.Application.Exceptions;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Domain.Constants;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Domain.Enums;
using foodiya.app.Identity.Infrastructure.Identity;
using foodiya.app.Identity.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace foodiya.app.Identity.Infrastructure.Authentication;

internal sealed class GoogleAccountLinker(UserManager<ApplicationUser> userManager, IdentityDataContext dataContext, TimeProvider timeProvider)
{
    internal const string GoogleLoginProvider = "Google";

    public async Task<ApplicationUser> ResolveAsync(
        GoogleIdentity identity,
        CancellationToken cancellationToken)
    {
        var email = identity.Email.Trim();
        if (email.Length > 256)
            throw new IdentityUnauthorizedException("GOOGLE_ID_TOKEN_INVALID", "The Google email address exceeds the supported length.");
        var now = timeProvider.GetUtcNow();

        await using var transaction = await dataContext.Database.BeginTransactionAsync(
            IsolationLevel.Serializable,
            cancellationToken);

        var alreadyLinked = await userManager.FindByLoginAsync(
            GoogleLoginProvider,
            identity.Subject);
        if (alreadyLinked is not null)
        {
            await transaction.CommitAsync(cancellationToken);
            return alreadyLinked;
        }

        var user = await userManager.FindByEmailAsync(email);
        var userRegistered = false;
        if (user is not null)
        {
            var googleLogin = (await userManager.GetLoginsAsync(user))
                .SingleOrDefault(login => string.Equals(
                    login.LoginProvider,
                    GoogleLoginProvider,
                    StringComparison.Ordinal));
            if (googleLogin is not null)
            {
                throw new IdentityConflictException(
                    "GOOGLE_ACCOUNT_ALREADY_LINKED",
                    "This account is already linked to a different Google identity.");
            }

            if (!IsGoogleAuthoritativeForEmail(identity))
            {
                throw new IdentityConflictException(
                    "GOOGLE_EMAIL_LINK_REQUIRES_VERIFICATION",
                    "This email already has an account. Sign in with its existing credentials before linking Google.");
            }

            if (user.Status == AccountStatus.Pending)
            {
                user.EmailConfirmed = true;
                user.Status = AccountStatus.Active;
                user.UpdatedAtUtc = now;
                // The unverified registration may have been created by someone else.
                // Remove its password and rotate the security stamp before activation commits.
                EnsureSucceeded(await userManager.RemovePasswordAsync(user));
                await dataContext.OtpChallenges
                    .Where(challenge => challenge.UserId == user.Id
                        && challenge.Purpose == OtpPurpose.Registration
                        && challenge.ConsumedAtUtc == null
                        && challenge.InvalidatedAtUtc == null)
                    .ExecuteUpdateAsync(
                        updates => updates.SetProperty(challenge => challenge.InvalidatedAtUtc, now),
                        cancellationToken);
                userRegistered = true;
            }
            else
            {
                IdentityAccountRules.EnsureCanAuthenticate(user);
            }
        }
        else
        {
            user = new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = await GenerateUniqueUserNameAsync(email),
                Email = email,
                FirstName = NormalizeGoogleName(identity.GivenName, "Google"),
                LastName = NormalizeGoogleName(identity.FamilyName, "User"),
                PreferredLanguage = "en",
                EmailConfirmed = true,
                Status = AccountStatus.Active,
                CreatedAtUtc = now,
                LockoutEnabled = true,
                SecurityStamp = Guid.NewGuid().ToString("N")
            };

            EnsureSucceeded(await userManager.CreateAsync(user));
            EnsureSucceeded(await userManager.AddToRoleAsync(user, RoleNames.User));
            userRegistered = true;
        }

        var loginResult = await userManager.AddLoginAsync(
            user,
            new UserLoginInfo(GoogleLoginProvider, identity.Subject, "Google"));
        if (!loginResult.Succeeded)
        {
            throw new IdentityConflictException(
                "GOOGLE_ACCOUNT_LINK_FAILED",
                "The Google identity could not be linked to this account.");
        }

        if (userRegistered)
        {
            dataContext.OutboxMessages.Add(new OutboxMessage
            {
                OccurredAtUtc = now,
                Type = "UserRegistered",
                Payload = JsonSerializer.Serialize(new
                {
                    user.Id,
                    user.Email,
                    user.UserName,
                    user.FirstName,
                    user.LastName,
                    Provider = GoogleLoginProvider
                })
            });
        }

        await dataContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return user;
    }

    private async Task<string> GenerateUniqueUserNameAsync(string email)
    {
        var localPart = email.Split('@', 2)[0].Trim().ToLowerInvariant();
        var sanitized = new string(localPart
            .Where(character => char.IsLetterOrDigit(character) || character is '.' or '-' or '_')
            .ToArray());
        var baseUserName = string.IsNullOrWhiteSpace(sanitized) ? "google-user" : sanitized;
        baseUserName = baseUserName[..Math.Min(baseUserName.Length, 40)];

        if (await userManager.FindByNameAsync(baseUserName) is null)
        {
            return baseUserName;
        }

        string candidate;
        do
        {
            candidate = $"{baseUserName}-{Guid.NewGuid():N}";
            candidate = candidate[..Math.Min(candidate.Length, 50)];
        }
        while (await userManager.FindByNameAsync(candidate) is not null);

        return candidate;
    }

    private static bool IsGoogleAuthoritativeForEmail(GoogleIdentity identity) =>
        identity.Email.EndsWith("@gmail.com", StringComparison.OrdinalIgnoreCase)
        || (identity.EmailVerified && !string.IsNullOrWhiteSpace(identity.HostedDomain));

    private static string NormalizeGoogleName(string? value, string fallback) =>
        string.IsNullOrWhiteSpace(value)
            ? fallback
            : value.Trim()[..Math.Min(value.Trim().Length, 100)];

    private static void EnsureSucceeded(IdentityResult result)
    {
        if (!result.Succeeded)
        {
            throw new InvalidOperationException("ASP.NET Core Identity could not update login state.");
        }
    }
}
