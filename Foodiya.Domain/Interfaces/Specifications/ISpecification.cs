using System.Linq.Expressions;

namespace Foodiya.Domain.Interfaces.Specifications;

/// <summary>
/// Encapsulates query criteria, includes, and ordering for a given entity type.
/// Infrastructure-agnostic — no EF Core types here.
/// </summary>
public interface ISpecification<T> where T : class
{
    /// <summary>
    /// Multiple where-clause expressions — all are AND-ed together.
    /// </summary>
    List<Expression<Func<T, bool>>> Criterias { get; }

    /// <summary>
    /// Expression-based includes captured as metadata trees.
    /// The infrastructure layer (SpecificationEvaluator) translates these into
    /// EF Core Include/ThenInclude calls.
    /// </summary>
    List<IncludeExpressionInfo> IncludeExpressions { get; }

    /// <summary>
    /// String-based includes (convenience shortcut for simple navigation paths).
    /// </summary>
    List<string> IncludeStrings { get; }

    Expression<Func<T, object>>? OrderBy { get; }
    Expression<Func<T, object>>? OrderByDescending { get; }
    Expression<Func<T, object>>? GroupBy { get; }

    int? Skip { get; set; }
    int? Take { get; set; }

    bool AsNoTracking { get; }
}
