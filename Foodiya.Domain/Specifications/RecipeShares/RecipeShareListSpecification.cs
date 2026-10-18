using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.RecipeShares;

/// <summary>
/// Specification: Paginated recipe share list with optional filters by recipe, users, channel, and free-text search.
/// </summary>
public sealed class RecipeShareListSpecification : BaseSpecification<RecipeShare>
{
    public RecipeShareListSpecification(
        int page,
        int pageSize,
        int? recipeId = null,
        int? sharedByUserId = null,
        int? sharedWithUserId = null,
        string? channel = null,
        string? search = null)
        : base(rs =>
            rs.Recipe.DeletedAt == null
            && (!recipeId.HasValue || rs.RecipeId == recipeId.Value)
            && (!sharedByUserId.HasValue || rs.SharedByUserId == sharedByUserId.Value)
            && (!sharedWithUserId.HasValue || rs.SharedWithUserId == sharedWithUserId.Value)
            && (string.IsNullOrWhiteSpace(channel) || rs.ShareChannel.ToLower() == channel.Trim().ToLower())
            && (string.IsNullOrWhiteSpace(search)
                || rs.Recipe.Title.ToLower().Contains(search.Trim().ToLower())
                || rs.SharedByUser.UserName.ToLower().Contains(search.Trim().ToLower())
                || rs.SharedByUser.FirstName.ToLower().Contains(search.Trim().ToLower())
                || rs.SharedByUser.LastName.ToLower().Contains(search.Trim().ToLower())
                || (rs.SharedWithUser != null && rs.SharedWithUser.UserName.ToLower().Contains(search.Trim().ToLower()))
                || (rs.SharedWithUser != null && rs.SharedWithUser.FirstName.ToLower().Contains(search.Trim().ToLower()))
                || (rs.SharedWithUser != null && rs.SharedWithUser.LastName.ToLower().Contains(search.Trim().ToLower()))
                || rs.ShareChannel.ToLower().Contains(search.Trim().ToLower())
                || (rs.ShareMessage != null && rs.ShareMessage.ToLower().Contains(search.Trim().ToLower()))))
    {
        AddInclude(rs => rs.Recipe);
        AddInclude(rs => rs.SharedByUser);
        AddInclude(rs => rs.SharedWithUser!);

        ApplyOrderByDescending(rs => rs.SharedAt);
        ApplyPaging((page - 1) * pageSize, pageSize);
    }
}
