using System.Linq.Expressions;

namespace Foodiya.Domain.Interfaces.Specifications;

/// <summary>
/// Represents a chain of Include / ThenInclude operations on a queryable.
/// This is a framework-agnostic abstraction — the actual IQueryable translation
/// happens in Infrastructure via SpecificationEvaluator.
/// </summary>
public interface IIncludeQuery<T> where T : class
{
    /// <summary>Chain a ThenInclude on a reference navigation.</summary>
    IIncludeQuery<T> ThenInclude<TPreviousProperty, TProperty>(
        Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath)
        where TProperty : class;

    /// <summary>Chain a ThenInclude on a collection navigation element.</summary>
    IIncludeQuery<T> ThenIncludeFromCollection<TPreviousProperty, TProperty>(
        Expression<Func<TPreviousProperty, TProperty>> navigationPropertyPath)
        where TProperty : class;
}

/// <summary>
/// Represents a single Include expression that can optionally be followed by ThenInclude chains.
/// Each Include is captured as a tree of expressions (not as IIncludableQueryable delegates)
/// so that the Domain layer stays free of EF Core.
/// </summary>
public sealed class IncludeExpressionInfo
{
    public LambdaExpression Expression { get; }
    public Type EntityType { get; }
    public Type PropertyType { get; }
    public IncludeExpressionInfo? PreviousInclude { get; }
    public bool IsCollection { get; }

    public IncludeExpressionInfo(
        LambdaExpression expression,
        Type entityType,
        Type propertyType,
        IncludeExpressionInfo? previousInclude = null,
        bool isCollection = false)
    {
        Expression = expression;
        EntityType = entityType;
        PropertyType = propertyType;
        PreviousInclude = previousInclude;
        IsCollection = isCollection;
    }
}
