using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.AppUsers;

/// <summary>
/// Specification: AppUser by email with chef profile data for auth flows.
/// </summary>
public sealed class AppUserByEmailSpecification : BaseSpecification<AppUser>
{
    public AppUserByEmailSpecification(string email)
        : base(user => user.Email.ToLower() == email.Trim().ToLower())
    {
        AddInclude(user => user.ChefProfileUser!);
    }
}
