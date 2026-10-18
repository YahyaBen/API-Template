using System.Linq.Expressions;
using Foodiya.Domain.Exceptions;
using Foodiya.Domain.Interfaces.Core;

namespace Foodiya.Application.Extensions;

public static class UuidRepositoryExtensions
{
    /// <summary>
    /// Builds a predicate <c>x => x.Uuid == uuid</c> at runtime using expression
    /// trees, so no <c>IHasUuid</c> generic constraint is needed.
    /// Throws <see cref="InvalidOperationException"/> at runtime if <typeparamref name="T"/>
    /// does not have a <c>Uuid</c> property of type <see cref="Guid"/>.
    /// </summary>
    private static Expression<Func<T, bool>> UuidPredicate<T>(Guid uuid) where T : class
    {
        var param = Expression.Parameter(typeof(T), "x");
        var property = Expression.Property(param, "Uuid");
        var constant = Expression.Constant(uuid);
        return Expression.Lambda<Func<T, bool>>(Expression.Equal(property, constant), param);
    }

    public static Task<T?> GetByUuidAsync<T>(
        this IGenericRepository<T> repository,
        Guid uuid,
        CancellationToken ct = default)
        where T : class
        => repository.GetSingleAsync(UuidPredicate<T>(uuid), ct);

    public static async Task<T> GetRequiredByUuidAsync<T>(
        this IGenericRepository<T> repository,
        Guid uuid,
        string entityName,
        CancellationToken ct = default)
        where T : class
    {
        return await repository.GetByUuidAsync(uuid, ct)
            ?? throw new FoodiyaNotFoundException($"{entityName} with UUID {uuid} not found.");
    }
}
