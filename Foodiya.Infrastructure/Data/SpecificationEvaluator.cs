using System.Linq.Expressions;
using Foodiya.Domain.Interfaces.Specifications;
using Microsoft.EntityFrameworkCore;

namespace Foodiya.Infrastructure.Data;

/// <summary>
/// Translates an <see cref="ISpecification{T}"/> into an EF Core <see cref="IQueryable{T}"/>.
/// Lives in Infrastructure because it depends on EF Core extension methods.
/// </summary>
public static class SpecificationEvaluator<T> where T : class
{
    public static IQueryable<T> GetQuery(IQueryable<T> inputQuery, ISpecification<T> spec)
    {
        if (spec == null)
            return inputQuery;

        var query = inputQuery;

        // AsNoTracking
        if (spec.AsNoTracking)
            query = query.AsNoTracking();

        // Expression-based includes (translated from domain IncludeExpressionInfo tree)
        query = ApplyIncludes(query, spec);

        // String-based includes (convenience for simple paths)
        query = spec.IncludeStrings
            .Aggregate(query, (current, include) => current.Include(include));

        // Apply ALL criteria (AND semantics)
        foreach (var criteria in spec.Criterias)
        {
            query = query.Where(criteria);
        }

        // Ordering
        if (spec.OrderBy is not null)
            query = query.OrderBy(spec.OrderBy);
        else if (spec.OrderByDescending is not null)
            query = query.OrderByDescending(spec.OrderByDescending);

        // GroupBy
        if (spec.GroupBy is not null)
            query = query.GroupBy(spec.GroupBy).SelectMany(x => x);

        return query.AsQueryable();
    }

    public static IQueryable<T> AddPagination(IQueryable<T> query, ISpecification<T> spec)
    {
        if (spec == null)
            return query;

        if (spec.Skip.HasValue)
            query = query.Skip(spec.Skip.Value);

        if (spec.Take.HasValue)
            query = query.Take(spec.Take.Value);

        return query;
    }

    /// <summary>
    /// Translates the specification's IncludeExpressionInfo list into EF Core Include/ThenInclude calls.
    /// Top-level includes (PreviousInclude == null) use EntityFrameworkQueryableExtensions.Include.
    /// Chained includes (PreviousInclude != null) use ThenInclude.
    /// </summary>
    private static IQueryable<T> ApplyIncludes(IQueryable<T> query, ISpecification<T> spec)
    {
        // We need to process includes in order, building up the chain.
        // Top-level includes have PreviousInclude == null.
        // ThenIncludes have PreviousInclude != null and reference the prior include info.

        // Because specs use the fluent builder, IncludeExpressions are in order:
        //   [0] Include(r => r.RecipeIngredients)          — top-level
        //   [1] ThenInclude(ri => ri.Ingredient)           — chained, PreviousInclude = [0]
        //   [2] Include(r => r.RecipeIngredients)          — top-level (new chain for same nav)
        //   [3] ThenInclude(ri => ri.Unit)                 — chained, PreviousInclude = [2]

        // We track the "current" IIncludableQueryable for each chain via dynamic dispatch.
        object currentIncludable = query;

        foreach (var includeInfo in spec.IncludeExpressions)
        {
            if (includeInfo.PreviousInclude == null)
            {
                // Top-level Include
                // Call: EntityFrameworkQueryableExtensions.Include<T, TProperty>(query, expression)
                currentIncludable = ApplyInclude(query, includeInfo);
                query = (IQueryable<T>)currentIncludable;
            }
            else
            {
                // ThenInclude — chained on the previous includable
                currentIncludable = ApplyThenInclude(currentIncludable, includeInfo);
                query = ExtractQueryable(currentIncludable);
            }
        }

        return query;
    }

    private static object ApplyInclude(IQueryable<T> query, IncludeExpressionInfo includeInfo)
    {
        // Build: query.Include(expression)
        // We use reflection to call the generic Include<T, TProperty> method.
        // Use the expression's actual return type so that collection navigations
        // (e.g. ICollection<FoodCategory>) are passed correctly to Include<T, TProperty>.
        var includeMethod = typeof(EntityFrameworkQueryableExtensions)
            .GetMethods()
            .First(m => m.Name == nameof(EntityFrameworkQueryableExtensions.Include)
                        && m.GetParameters().Length == 2
                        && m.GetParameters()[0].ParameterType.GetGenericTypeDefinition() == typeof(IQueryable<>))
            .MakeGenericMethod(typeof(T), includeInfo.Expression.ReturnType);

        return includeMethod.Invoke(null, [query, includeInfo.Expression])!;
    }

    private static object ApplyThenInclude(object previousIncludable, IncludeExpressionInfo includeInfo)
    {
        var previousPropertyType = includeInfo.PreviousInclude!.PropertyType;
        var isCollection = includeInfo.PreviousInclude.IsCollection;

        // Pick the right ThenInclude overload based on whether the previous include was a collection
        var thenIncludeMethods = typeof(EntityFrameworkQueryableExtensions)
            .GetMethods()
            .Where(m => m.Name == nameof(EntityFrameworkQueryableExtensions.ThenInclude)
                        && m.GetParameters().Length == 2);

        System.Reflection.MethodInfo thenIncludeMethod;

        if (isCollection)
        {
            // ThenInclude<TEntity, TPreviousProperty, TProperty>(
            //   IIncludableQueryable<TEntity, IEnumerable<TPreviousProperty>>, ...)
            thenIncludeMethod = thenIncludeMethods
                .First(m =>
                {
                    var param = m.GetParameters()[0].ParameterType;
                    if (!param.IsGenericType) return false;
                    var args = param.GetGenericArguments();
                    return args.Length == 2 && args[1].IsGenericType
                        && args[1].GetGenericTypeDefinition() == typeof(IEnumerable<>);
                })
                .MakeGenericMethod(typeof(T), previousPropertyType, includeInfo.PropertyType);
        }
        else
        {
            // ThenInclude<TEntity, TPreviousProperty, TProperty>(
            //   IIncludableQueryable<TEntity, TPreviousProperty>, ...)
            thenIncludeMethod = thenIncludeMethods
                .First(m =>
                {
                    var param = m.GetParameters()[0].ParameterType;
                    if (!param.IsGenericType) return false;
                    var args = param.GetGenericArguments();
                    return args.Length == 2 && !args[1].IsGenericType;
                })
                .MakeGenericMethod(typeof(T), previousPropertyType, includeInfo.PropertyType);
        }

        return thenIncludeMethod.Invoke(null, [previousIncludable, includeInfo.Expression])!;
    }

    private static IQueryable<T> ExtractQueryable(object includable)
    {
        // IIncludableQueryable<T, X> implements IQueryable<T>
        return (IQueryable<T>)includable;
    }
}
