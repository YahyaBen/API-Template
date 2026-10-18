using Foodiya.Domain.Models;

namespace Foodiya.Domain.Interfaces.Core;

/// <summary>
/// Repository for AppUser external login records.
/// </summary>
public interface IAppUserExternalLoginRepository : IGenericRepository<AppUserExternalLogin>
{
}
