using System.Linq.Expressions;
using Foodiya.Domain.Interfaces.Specifications;

namespace Foodiya.Domain.Interfaces.Core;

/// <summary>
/// Generic repository interface.
/// Provides both sync and async data access, specification-based querying,
/// projected mapping, paginated results, and transaction support.
/// Depends only on Domain — no EF references here.
/// </summary>
public interface IGenericRepository<T> where T : class
{
    // ─── Sync ───────────────────────────────────────────────────

    T? GetById(int id, ISpecification<T>? specification = null);

    T? GetSingle(ISpecification<T> specification);

    IQueryable<T> GetAll(ISpecification<T>? specification = null);

    IEnumerable<TProjectTo> GetAll<TProjectTo>(ISpecification<T>? specification = null);

    // ─── Async ──────────────────────────────────────────────────

    Task<T?> GetByIdAsync(int id, ISpecification<T>? specification = null, CancellationToken ct = default);

    Task<T?> GetSingleAsync(ISpecification<T> specification, CancellationToken ct = default);

    Task<T?> GetSingleAsync(Expression<Func<T, bool>> predicate, CancellationToken ct = default);

    Task<int> CountAsync(ISpecification<T> specification, CancellationToken ct = default);

    Task<IReadOnlyList<T>> ListAsync(ISpecification<T> specification, CancellationToken ct = default);

    Task<(IEnumerable<TProjectTo> results, int itemsCount)> GetAllPaginated<TProjectTo>(ISpecification<T> specification, CancellationToken ct = default);

    // ─── Commands ───────────────────────────────────────────────

    Task<T> InsertAsync(T entity, CancellationToken ct = default);

    T Update(T entity);

    void Delete(T entity);

    Task<int> SaveAsync(CancellationToken ct = default);

    Task<ITransaction> BeginTransactionAsync(CancellationToken ct = default);
}
