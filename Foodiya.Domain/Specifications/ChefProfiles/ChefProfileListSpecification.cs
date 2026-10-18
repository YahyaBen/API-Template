using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.ChefProfiles;

/// <summary>
/// Specification: Paginated chef profile list with optional verified filter and free-text search.
/// </summary>
public sealed class ChefProfileListSpecification : BaseSpecification<ChefProfile>
{
    public ChefProfileListSpecification(
        int page,
        int pageSize,
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
        AddInclude(cp => cp.User);

        ApplyOrderBy(cp => cp.DisplayName);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
