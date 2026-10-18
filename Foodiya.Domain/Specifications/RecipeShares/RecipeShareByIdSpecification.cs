using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;
namespace Foodiya.Domain.Specifications.RecipeShares;

/// <summary>
/// Specification: Recipe share detail by ID with linked recipe and users.
/// </summary>
public sealed class RecipeShareByIdSpecification : BaseSpecification<RecipeShare>
{
    public RecipeShareByIdSpecification(int id) : base(rs => rs.Id == id)
    {
        AddInclude(rs => rs.Recipe);
        AddInclude(rs => rs.SharedByUser);
        AddInclude(rs => rs.SharedWithUser!);
    }
}
