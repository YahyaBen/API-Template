using Foodiya.API.Controllers.Common;
using Foodiya.Application.DTOs.ReferenceData.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Foodiya.API.Controllers
{
    public sealed class ReferentialController : BaseController
    {
        private readonly IReferenceDataService _referenceDataService;

        public ReferentialController(IReferenceDataService referenceDataService)
        {
            _referenceDataService = referenceDataService ?? throw new FoodiyaNullArgumentException(nameof(referenceDataService));
        }

        [HttpGet(Name = "GetReferenceData")]
        [ProducesResponseType(typeof(ReferenceDataResponse), StatusCodes.Status200OK)]
        public async Task<ActionResult<ReferenceDataResponse>> Get(CancellationToken cancellationToken)
        {
            var result = await _referenceDataService.GetAsync(cancellationToken);
            return Ok(result);
        }
    }
}
