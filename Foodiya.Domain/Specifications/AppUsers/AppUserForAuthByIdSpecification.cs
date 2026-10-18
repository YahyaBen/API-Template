using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.AppUsers;

/// <summary>
/// Specification: AppUser detail for auth/profile responses.
/// </summary>
public sealed class AppUserForAuthByIdSpecification : BaseSpecification<AppUser>
{
    public AppUserForAuthByIdSpecification(int userId)
        : base(user => user.Id == userId)
    {
        AddInclude(user => user.ChefProfileUser!);
    }
}
