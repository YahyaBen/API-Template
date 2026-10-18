using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.ChefProfiles;

/// <summary>
/// Specification: Find the chef profile that belongs to a specific AppUser.
/// </summary>
public sealed class ChefProfileByUserIdSpecification : BaseSpecification<ChefProfile>
{
    public ChefProfileByUserIdSpecification(int userId) : base(cp => cp.UserId == userId && cp.DeletedAt == null)
    {
    }
}
