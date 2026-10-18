using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.ChefProfiles;

/// <summary>
/// Specification: Chef profile detail by ID with linked AppUser data.
/// </summary>
public sealed class ChefProfileByIdSpecification : BaseSpecification<ChefProfile>
{
    public ChefProfileByIdSpecification(int id) : base(cp => cp.Id == id && cp.DeletedAt == null)
    {
        AddInclude(cp => cp.User);
    }
}
