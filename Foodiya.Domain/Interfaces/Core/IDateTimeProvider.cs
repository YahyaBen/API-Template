namespace Foodiya.Domain.Interfaces.Core;

/// <summary>
/// Abstraction over system clock to make time-dependent code testable.
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
