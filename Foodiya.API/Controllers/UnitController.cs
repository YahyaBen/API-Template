using Foodiya.API.Controllers.Common;
using Foodiya.Domain.Constants;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Application.DTOs.Unit.Request;
using Foodiya.Application.DTOs.Unit.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foodiya.API.Controllers;

public sealed class UnitController : BaseController
{
    private readonly IUnitService _unitService;

    public UnitController(IUnitService unitService)
    {
        _unitService = unitService ?? throw new FoodiyaNullArgumentException(nameof(unitService));
    }

    /// <summary>
    /// Create a new unit
    /// </summary>
    /// <remarks>
    /// Creates a new unit entry. Code should be unique.
    /// </remarks>
    /// <param name="request">Unit payload</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPost]
    [ProducesResponseType(typeof(UnitDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UnitDetailResponse>> Create([FromBody] CreateUnitRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _unitService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { uuid = created.Uuid }, created);
    }


    /// <summary>
    /// Update an existing unit
    /// </summary>
    /// <remarks>
    /// Updates the editable unit fields.
    /// </remarks>
    /// <param name="uuid">Unit UUID</param>
    /// <param name="request">Updated unit payload</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPatch("{uuid:guid}")]
    [ProducesResponseType(typeof(UnitDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UnitDetailResponse>> Update(Guid uuid, [FromBody] UpdateUnitRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _unitService.UpdateAsync(uuid, request, ct);
        return Ok(updated);
    }


    /// <summary>
    /// Get a unit by UUID
    /// </summary>
    /// <remarks>
    /// Returns the full unit details.
    /// </remarks>
    /// <param name="uuid">Unit UUID</param>
    [HttpGet("{uuid:guid}")]
    [ProducesResponseType(typeof(UnitDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UnitDetailResponse>> GetById(Guid uuid, CancellationToken ct)
    {
        var unit = await _unitService.GetByIdAsync(uuid, ct);
        return unit is null ? NotFound() : Ok(unit);
    }


    /// <summary>
    /// Get all units
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of units with optional filters by active status or free-text search.
    /// </remarks>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 12)</param>
    /// <param name="isActive">Filter by active status (optional)</param>
    /// <param name="search">Search on unit label or code (optional)</param>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<UnitDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<UnitDetailResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _unitService.ListAsync(page, pageSize, isActive, search, ct);
        return Ok(result);
    }


    /// <summary>
    /// Toggle unit active status
    /// </summary>
    /// <remarks>
    /// Flips the IsActive flag for the target unit.
    /// </remarks>
    /// <param name="uuid">Unit UUID</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPatch("{uuid:guid}/toggle-active")]
    [ProducesResponseType(typeof(UnitDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<UnitDetailResponse>> ToggleActive(Guid uuid, CancellationToken ct)
    {
        var unit = await _unitService.ToggleActiveAsync(uuid, ct);
        return Ok(unit);
    }


    /// <summary>
    /// Delete a unit
    /// </summary>
    /// <remarks>
    /// Deletes the unit when it is not referenced by ingredients or recipe ingredients.
    /// </remarks>
    /// <param name="uuid">Unit UUID</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpDelete("{uuid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid uuid, CancellationToken ct)
    {
        await _unitService.DeleteAsync(uuid, ct);
        return NoContent();
    }

}
