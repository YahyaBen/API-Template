using System.Linq.Expressions;

namespace Foodiya.Domain.Interfaces.Specifications;

/// <summary>
/// Base implementation of ISpecification. Extend this class to create concrete specifications.
/// Infrastructure-agnostic — no EF Core types here.
/// </summary>
public abstract class BaseSpecification<T> : ISpecification<T> where T : class
{
    #region Properties

    public List<Expression<Func<T, bool>>> Criterias { get; } = [];
    public List<IncludeExpressionInfo> IncludeExpressions { get; } = [];
    public List<string> IncludeStrings { get; } = [];
    public Expression<Func<T, object>>? OrderBy { get; protected set; }
    public Expression<Func<T, object>>? OrderByDescending { get; protected set; }
    public Expression<Func<T, object>>? GroupBy { get; protected set; }
    public int? Skip { get; set; }
    public int? Take { get; set; }
    public bool AsNoTracking { get; private set; } = true;

    #endregion

    #region Constructors

    protected BaseSpecification() { }

    /// <summary>
    /// Convenience: pass a single criterion — it is added to the Criterias list.
    /// </summary>
    protected BaseSpecification(Expression<Func<T, bool>> criteria)
    {
        Criterias.Add(criteria);
    }

    #endregion

    #region Methods

    /// <summary>
    /// Add a top-level Include for a reference navigation property.
    /// Returns an <see cref="IncludeBuilder{T, TProperty}"/> to allow chaining ThenInclude.
    /// </summary>
    protected IncludeBuilder<T, TProperty> AddInclude<TProperty>(
        Expression<Func<T, TProperty>> navigationPropertyPath)
        where TProperty : class
    {
        var info = new IncludeExpressionInfo(
            navigationPropertyPath,
            typeof(T),
            typeof(TProperty));
        IncludeExpressions.Add(info);
        return new IncludeBuilder<T, TProperty>(this, info);
    }

    /// <summary>
    /// Add a top-level Include for a collection navigation property (ICollection&lt;TElement&gt;).
    /// Returns an <see cref="IncludeBuilder{T, TElement}"/> so ThenInclude operates on the element type.
    /// </summary>
    protected IncludeBuilder<T, TElement> AddInclude<TElement>(
        Expression<Func<T, ICollection<TElement>>> navigationPropertyPath)
        where TElement : class
    {
        var info = new IncludeExpressionInfo(
            navigationPropertyPath,
            typeof(T),
            typeof(TElement),
            isCollection: true);
        IncludeExpressions.Add(info);
        return new IncludeBuilder<T, TElement>(this, info);
    }

    /// <summary>
    /// Add a top-level Include for an IEnumerable collection navigation property.
    /// Returns an <see cref="IncludeBuilder{T, TElement}"/> so ThenInclude operates on the element type.
    /// </summary>
    protected IncludeBuilder<T, TElement> AddInclude<TElement>(
        Expression<Func<T, IEnumerable<TElement>>> navigationPropertyPath)
        where TElement : class
    {
        var info = new IncludeExpressionInfo(
            navigationPropertyPath,
            typeof(T),
            typeof(TElement),
            isCollection: true);
        IncludeExpressions.Add(info);
        return new IncludeBuilder<T, TElement>(this, info);
    }

    /// <summary>
    /// Add a string-based include for simple navigation paths (e.g. "RecipeIngredients.Ingredient").
    /// </summary>
    protected void AddInclude(string includeString)
        => IncludeStrings.Add(includeString);

    protected void AddCriteria(Expression<Func<T, bool>> criteria)
        => Criterias.Add(criteria);

    protected void ApplyOrderBy(Expression<Func<T, object>> orderByExpression)
        => OrderBy = orderByExpression;

    protected void AddOrderBy(Expression<Func<T, object>> orderByExpression)
        => OrderBy = orderByExpression;

    protected void ApplyOrderByDescending(Expression<Func<T, object>> orderByDescExpression)
        => OrderByDescending = orderByDescExpression;

    protected void ApplyPaging(int skip, int take)
    {
        Skip = skip;
        Take = take;
    }

    protected void ApplyTracking()
        => AsNoTracking = false;

    #endregion
}

/// <summary>
/// Fluent builder for chaining ThenInclude calls in a type-safe, EF-Core-free way.
/// </summary>
public sealed class IncludeBuilder<TEntity, TProperty>
    where TEntity : class
    where TProperty : class
{
    private readonly BaseSpecification<TEntity> _spec;
    private readonly IncludeExpressionInfo _lastInclude;

    internal IncludeBuilder(BaseSpecification<TEntity> spec, IncludeExpressionInfo lastInclude)
    {
        _spec = spec;
        _lastInclude = lastInclude;
    }

    /// <summary>
    /// Chain a ThenInclude on a reference navigation.
    /// </summary>
    public IncludeBuilder<TEntity, TNext> ThenInclude<TNext>(
        Expression<Func<TProperty, TNext>> navigationPropertyPath)
        where TNext : class
    {
        var info = new IncludeExpressionInfo(
            navigationPropertyPath,
            typeof(TProperty),
            typeof(TNext),
            previousInclude: _lastInclude);
        _spec.IncludeExpressions.Add(info);
        return new IncludeBuilder<TEntity, TNext>(_spec, info);
    }

    /// <summary>
    /// Chain a ThenInclude on a collection navigation.
    /// </summary>
    public IncludeBuilder<TEntity, TNext> ThenInclude<TNext>(
        Expression<Func<TProperty, IEnumerable<TNext>>> navigationPropertyPath)
        where TNext : class
    {
        var info = new IncludeExpressionInfo(
            navigationPropertyPath,
            typeof(TProperty),
            typeof(TNext),
            previousInclude: _lastInclude,
            isCollection: true);
        _spec.IncludeExpressions.Add(info);
        return new IncludeBuilder<TEntity, TNext>(_spec, info);
    }
}
