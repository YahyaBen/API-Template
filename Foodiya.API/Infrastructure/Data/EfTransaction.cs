using Foodiya.Domain.Interfaces.Core;
using Microsoft.EntityFrameworkCore.Storage;

namespace Foodiya.Infrastructure.Data;

/// <summary>
/// Wraps an EF Core <see cref="IDbContextTransaction"/> behind the domain's
/// <see cref="ITransaction"/> abstraction.
/// </summary>
internal sealed class EfTransaction : ITransaction
{
    private readonly IDbContextTransaction _inner;

    public EfTransaction(IDbContextTransaction inner)
    {
        _inner = inner;
    }

    public Task CommitAsync(CancellationToken cancellationToken = default)
        => _inner.CommitAsync(cancellationToken);

    public Task RollbackAsync(CancellationToken cancellationToken = default)
        => _inner.RollbackAsync(cancellationToken);

    public ValueTask DisposeAsync()
        => _inner.DisposeAsync();
}
