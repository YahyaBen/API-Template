namespace Foodiya.Domain.Interfaces.Core;

/// <summary>
/// Represents a database transaction abstraction for the domain layer.
/// Infrastructure provides the concrete implementation (e.g., EF Core DbContextTransaction).
/// </summary>
public interface ITransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
    Task RollbackAsync(CancellationToken cancellationToken = default);
}
