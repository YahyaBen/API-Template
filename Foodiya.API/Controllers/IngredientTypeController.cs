using Foodiya.API.Controllers.Common;
using Foodiya.Domain.Constants;
using Foodiya.Application.DTOs.IngredientType.Request;
using Foodiya.Application.DTOs.IngredientType.Response;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foodiya.API.Controllers;

public sealed class IngredientTypeController : BaseController
{
    private readonly IIngredientTypeService _ingredientTypeService;

    public IngredientTypeController(IIngredientTypeService ingredientTypeService)
    {
        _ingredientTypeService = ingredientTypeService ?? throw new FoodiyaNullArgumentException(nameof(ingredientTypeService));
    }

    /// <summary>
    /// Create a new ingredient type
    /// </summary>
    /// <remarks>
    /// Creates a new ingredient type entry. Code should be unique.
    /// </remarks>
    /// <param name="request">Ingredient type payload</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPost]
    [ProducesResponseType(typeof(IngredientTypeDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IngredientTypeDetailResponse>> Create([FromBody] CreateIngredientTypeRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _ingredientTypeService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { uuid = created.Uuid }, created);
    }


    /// <summary>
    /// Update an existing ingredient type
    /// </summary>
    /// <remarks>
    /// Updates the editable ingredient type fields. Optional text fields can be cleared by sending an empty string.
    /// </remarks>
    /// <param name="uuid">Ingredient type UUID</param>
    /// <param name="request">Updated ingredient type payload</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPatch("{uuid:guid}")]
    [ProducesResponseType(typeof(IngredientTypeDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IngredientTypeDetailResponse>> Update(Guid uuid, [FromBody] UpdateIngredientTypeRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _ingredientTypeService.UpdateAsync(uuid, request, ct);
        return Ok(updated);
    }


    /// <summary>
    /// Get an ingredient type by UUID
    /// </summary>
    /// <remarks>
    /// Returns the full ingredient type details.
    /// </remarks>
    /// <param name="uuid">Ingredient type UUID</param>
    [HttpGet("{uuid:guid}")]
    [ProducesResponseType(typeof(IngredientTypeDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IngredientTypeDetailResponse>> GetById(Guid uuid, CancellationToken ct)
    {
        var ingredientType = await _ingredientTypeService.GetByIdAsync(uuid, ct);
        return ingredientType is null ? NotFound() : Ok(ingredientType);
    }


    /// <summary>
    /// Get all ingredient types
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of ingredient types with optional filters by active status or free-text search.
    /// </remarks>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 12)</param>
    /// <param name="isActive">Filter by active status (optional)</param>
    /// <param name="search">Search on ingredient type label or code (optional)</param>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<IngredientTypeDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<IngredientTypeDetailResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _ingredientTypeService.ListAsync(page, pageSize, isActive, search, ct);
        return Ok(result);
    }


    /// <summary>
    /// Toggle ingredient type active status
    /// </summary>
    /// <remarks>
    /// Flips the IsActive flag for the target ingredient type.
    /// </remarks>
    /// <param name="uuid">Ingredient type UUID</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPatch("{uuid:guid}/toggle-active")]
    [ProducesResponseType(typeof(IngredientTypeDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IngredientTypeDetailResponse>> ToggleActive(Guid uuid, CancellationToken ct)
    {
        var ingredientType = await _ingredientTypeService.ToggleActiveAsync(uuid, ct);
        return Ok(ingredientType);
    }


    /// <summary>
    /// Delete an ingredient type
    /// </summary>
    /// <remarks>
    /// Deletes the ingredient type when it is not referenced by ingredients.
    /// </remarks>
    /// <param name="uuid">Ingredient type UUID</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpDelete("{uuid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid uuid, CancellationToken ct)
    {
        await _ingredientTypeService.DeleteAsync(uuid, ct);
        return NoContent();
    }

}
