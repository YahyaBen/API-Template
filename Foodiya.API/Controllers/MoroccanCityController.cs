using Foodiya.API.Controllers.Common;
using Foodiya.Domain.Constants;
using Foodiya.Application.DTOs.MoroccanCity.Request;
using Foodiya.Application.DTOs.MoroccanCity.Response;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foodiya.API.Controllers;

public sealed class MoroccanCityController : BaseController
{
    private readonly IMoroccanCityService _moroccanCityService;

    public MoroccanCityController(IMoroccanCityService moroccanCityService)
    {
        _moroccanCityService = moroccanCityService ?? throw new FoodiyaNullArgumentException(nameof(moroccanCityService));
    }

    /// <summary>
    /// Create a new Moroccan city
    /// </summary>
    /// <remarks>
    /// Creates a new Moroccan city entry. The slug is generated automatically from the name.
    /// </remarks>
    /// <param name="request">Moroccan city payload</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPost]
    [ProducesResponseType(typeof(MoroccanCityDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MoroccanCityDetailResponse>> Create([FromBody] CreateMoroccanCityRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _moroccanCityService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { uuid = created.Uuid }, created);
    }


    /// <summary>
    /// Update an existing Moroccan city
    /// </summary>
    /// <remarks>
    /// Updates the editable city fields. If the name changes, the slug is regenerated automatically.
    /// </remarks>
    /// <param name="uuid">Moroccan city UUID</param>
    /// <param name="request">Updated Moroccan city payload</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPatch("{uuid:guid}")]
    [ProducesResponseType(typeof(MoroccanCityDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MoroccanCityDetailResponse>> Update(Guid uuid, [FromBody] UpdateMoroccanCityRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _moroccanCityService.UpdateAsync(uuid, request, ct);
        return Ok(updated);
    }


    /// <summary>
    /// Get a Moroccan city by UUID
    /// </summary>
    /// <remarks>
    /// Returns the full Moroccan city details together with the linked region name.
    /// </remarks>
    /// <param name="uuid">Moroccan city UUID</param>
    [HttpGet("{uuid:guid}")]
    [ProducesResponseType(typeof(MoroccanCityDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MoroccanCityDetailResponse>> GetById(Guid uuid, CancellationToken ct)
    {
        var city = await _moroccanCityService.GetByIdAsync(uuid, ct);
        return city is null ? NotFound() : Ok(city);
    }


    /// <summary>
    /// Get all Moroccan cities
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of Moroccan cities with optional filters by region, active status, or free-text search.
    /// </remarks>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 12)</param>
    /// <param name="regionUuid">Filter by region UUID (optional)</param>
    /// <param name="isActive">Filter by active status (optional)</param>
    /// <param name="search">Search on city name, slug, or region name (optional)</param>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<MoroccanCityDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<MoroccanCityDetailResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] Guid? regionUuid = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _moroccanCityService.ListAsync(page, pageSize, regionUuid, isActive, search, ct);
        return Ok(result);
    }


    /// <summary>
    /// Toggle Moroccan city active status
    /// </summary>
    /// <remarks>
    /// Flips the IsActive flag for the target Moroccan city.
    /// </remarks>
    /// <param name="uuid">Moroccan city UUID</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPatch("{uuid:guid}/toggle-active")]
    [ProducesResponseType(typeof(MoroccanCityDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MoroccanCityDetailResponse>> ToggleActive(Guid uuid, CancellationToken ct)
    {
        var city = await _moroccanCityService.ToggleActiveAsync(uuid, ct);
        return Ok(city);
    }


    /// <summary>
    /// Delete a Moroccan city
    /// </summary>
    /// <remarks>
    /// Deletes the city when it is not referenced by recipes.
    /// </remarks>
    /// <param name="uuid">Moroccan city UUID</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpDelete("{uuid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid uuid, CancellationToken ct)
    {
        await _moroccanCityService.DeleteAsync(uuid, ct);
        return NoContent();
    }

}
