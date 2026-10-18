using Foodiya.Domain.Interfaces.Specifications;
using Foodiya.Domain.Models;

namespace Foodiya.Domain.Specifications.Difficulties;

/// <summary>
/// Specification: Difficulty detail by ID.
/// </summary>
public sealed class DifficultyByIdSpecification : BaseSpecification<Difficulty>
{
    public DifficultyByIdSpecification(int id) : base(d => d.Id == id)
    {
    }
}
