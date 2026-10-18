using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.AppUsers;

/// <summary>
/// Specification: External login lookup by internal user and provider.
/// </summary>
public sealed class AppUserExternalLoginByUserAndProviderSpecification : BaseSpecification<AppUserExternalLogin>
{
    public AppUserExternalLoginByUserAndProviderSpecification(int userId, string provider)
        : base(login => login.AppUserId == userId && login.Provider == provider)
    {
    }
}
