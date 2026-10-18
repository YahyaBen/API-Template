using Foodiya.Domain.Interfaces.Core;

namespace Foodiya.Infrastructure.Services;

/// <summary>
/// Production implementation — delegates to the real system clock.
/// </summary>
internal sealed class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
