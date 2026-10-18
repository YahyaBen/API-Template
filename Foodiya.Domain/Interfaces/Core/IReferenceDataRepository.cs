using Foodiya.Domain.Models;

namespace Foodiya.Domain.Interfaces.Core;

/// <summary>
/// Specialized repository for fetching all reference/lookup data in a single call.
/// </summary>
public interface IReferenceDataRepository
{
    Task<ReferenceData> GetAllReferenceDataAsync(CancellationToken ct = default);
}
