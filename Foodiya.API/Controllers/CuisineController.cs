using Foodiya.API.Controllers.Common;
using Foodiya.Domain.Constants;
using Foodiya.Application.DTOs.Cuisine.Request;
using Foodiya.Application.DTOs.Cuisine.Response;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foodiya.API.Controllers;

public sealed class CuisineController : BaseController
{
    private readonly ICuisineService _cuisineService;

    public CuisineController(ICuisineService cuisineService)
    {
        _cuisineService = cuisineService ?? throw new FoodiyaNullArgumentException(nameof(cuisineService));
    }

    /// <summary>
    /// Create a new cuisine
    /// </summary>
    /// <remarks>
    /// Creates a new cuisine entry. Name and code should be unique.
    /// </remarks>
    /// <param name="request">Cuisine payload</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPost]
    [ProducesResponseType(typeof(CuisineDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CuisineDetailResponse>> Create([FromBody] CreateCuisineRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _cuisineService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { uuid = created.Uuid }, created);
    }


    /// <summary>
    /// Update an existing cuisine
    /// </summary>
    /// <remarks>
    /// Updates the editable cuisine fields. Optional text fields can be cleared by sending an empty string.
    /// </remarks>
    /// <param name="uuid">Cuisine UUID</param>
    /// <param name="request">Updated cuisine payload</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPatch("{uuid:guid}")]
    [ProducesResponseType(typeof(CuisineDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CuisineDetailResponse>> Update(Guid uuid, [FromBody] UpdateCuisineRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _cuisineService.UpdateAsync(uuid, request, ct);
        return Ok(updated);
    }


    /// <summary>
    /// Get a cuisine by UUID
    /// </summary>
    /// <remarks>
    /// Returns the full cuisine details.
    /// </remarks>
    /// <param name="uuid">Cuisine UUID</param>
    [HttpGet("{uuid:guid}")]
    [ProducesResponseType(typeof(CuisineDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CuisineDetailResponse>> GetById(Guid uuid, CancellationToken ct)
    {
        var cuisine = await _cuisineService.GetByIdAsync(uuid, ct);
        return cuisine is null ? NotFound() : Ok(cuisine);
    }


    /// <summary>
    /// Get all cuisines
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of cuisines with optional filters by active status or free-text search.
    /// </remarks>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 12)</param>
    /// <param name="isActive">Filter by active status (optional)</param>
    /// <param name="search">Search on cuisine name or code (optional)</param>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<CuisineDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<CuisineDetailResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _cuisineService.ListAsync(page, pageSize, isActive, search, ct);
        return Ok(result);
    }


    /// <summary>
    /// Toggle cuisine active status
    /// </summary>
    /// <remarks>
    /// Flips the IsActive flag for the target cuisine.
    /// </remarks>
    /// <param name="uuid">Cuisine UUID</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPatch("{uuid:guid}/toggle-active")]
    [ProducesResponseType(typeof(CuisineDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<CuisineDetailResponse>> ToggleActive(Guid uuid, CancellationToken ct)
    {
        var cuisine = await _cuisineService.ToggleActiveAsync(uuid, ct);
        return Ok(cuisine);
    }


    /// <summary>
    /// Delete a cuisine
    /// </summary>
    /// <remarks>
    /// Deletes the cuisine when it is not referenced by recipes.
    /// </remarks>
    /// <param name="uuid">Cuisine UUID</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpDelete("{uuid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid uuid, CancellationToken ct)
    {
        await _cuisineService.DeleteAsync(uuid, ct);
        return NoContent();
    }

}
