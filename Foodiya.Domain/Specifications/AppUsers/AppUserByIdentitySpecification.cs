using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.AppUsers;

/// <summary>
/// Specification: AppUser by email or username with chef profile data for authentication.
/// </summary>
public sealed class AppUserByIdentitySpecification : BaseSpecification<AppUser>
{
    public AppUserByIdentitySpecification(string identity)
        : base(user =>
            user.Email.ToLower() == identity.Trim().ToLower()
            || user.UserName.ToLower() == identity.Trim().ToLower())
    {
        AddInclude(user => user.ChefProfileUser!);
    }
}
