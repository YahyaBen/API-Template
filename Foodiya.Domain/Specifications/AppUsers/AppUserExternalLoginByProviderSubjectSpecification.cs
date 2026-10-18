using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.AppUsers;

/// <summary>
/// Specification: External login lookup by provider and provider subject.
/// </summary>
public sealed class AppUserExternalLoginByProviderSubjectSpecification : BaseSpecification<AppUserExternalLogin>
{
    public AppUserExternalLoginByProviderSubjectSpecification(string provider, string providerSubject)
        : base(login =>
            login.Provider == provider
            && login.ProviderSubject == providerSubject)
    {
        AddInclude(login => login.AppUser)
                         .ThenInclude(user => user.ChefProfileUser!);
    }
}
