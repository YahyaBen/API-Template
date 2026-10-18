using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.ChefProfiles;

/// <summary>
/// Specification: Count chef profiles using the same filters as ChefProfileListSpecification.
/// </summary>
public sealed class ChefProfileCountSpecification : BaseSpecification<ChefProfile>
{
    public ChefProfileCountSpecification(
        bool? isVerified = null,
        string? search = null)
        : base(cp =>
            cp.DeletedAt == null
            && (!isVerified.HasValue || cp.IsVerified == isVerified.Value)
            && (string.IsNullOrWhiteSpace(search)
                || cp.DisplayName.ToLower().Contains(search.Trim().ToLower())
                || (cp.Bio != null && cp.Bio.ToLower().Contains(search.Trim().ToLower()))
                || (cp.Specialty != null && cp.Specialty.ToLower().Contains(search.Trim().ToLower()))
                || cp.User.UserName.ToLower().Contains(search.Trim().ToLower())
                || cp.User.FirstName.ToLower().Contains(search.Trim().ToLower())
                || cp.User.LastName.ToLower().Contains(search.Trim().ToLower())))
    {
    }
}
