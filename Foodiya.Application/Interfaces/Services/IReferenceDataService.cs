using Foodiya.Application.DTOs.ReferenceData.Response;

namespace Foodiya.Application.Interfaces.Services;

public interface IReferenceDataService
{
    Task<ReferenceDataResponse> GetAsync(CancellationToken cancellationToken = default);
}
