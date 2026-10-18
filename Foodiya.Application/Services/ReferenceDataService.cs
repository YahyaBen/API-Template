using AutoMapper;
using Foodiya.Application.DTOs.ReferenceData.Response;
using Foodiya.Application.Interfaces.Services;
using Foodiya.Domain.Interfaces.Core;

namespace Foodiya.Application.Services;

public sealed class ReferenceDataService : IReferenceDataService
{
    private readonly IReferenceDataRepository _referenceDataRepository;
    private readonly IMapper _mapper;

    public ReferenceDataService(IReferenceDataRepository referenceDataRepository, IMapper mapper)
    {
        _referenceDataRepository = referenceDataRepository;
        _mapper = mapper;
    }

    public async Task<ReferenceDataResponse> GetAsync(CancellationToken cancellationToken = default)
    {
        var referenceData = await _referenceDataRepository.GetAllReferenceDataAsync(cancellationToken);
        return _mapper.Map<ReferenceDataResponse>(referenceData);
    }
}
