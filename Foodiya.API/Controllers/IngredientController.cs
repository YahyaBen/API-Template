using Foodiya.API.Controllers.Common;
using Foodiya.Domain.Constants;
using Foodiya.Application.DTOs.Ingredient.Request;
using Foodiya.Application.DTOs.Ingredient.Response;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foodiya.API.Controllers;

public sealed class IngredientController : BaseController
{
    private readonly IIngredientService _ingredientService;

    public IngredientController(IIngredientService ingredientService)
    {
        _ingredientService = ingredientService ?? throw new FoodiyaNullArgumentException(nameof(ingredientService));
    }

    /// <summary>
    /// Create a new ingredient
    /// </summary>
    /// <remarks>
    /// Creates a new ingredient and validates the referenced ingredient type and default unit.
    /// </remarks>
    /// <param name="request">Ingredient payload</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPost]
    [ProducesResponseType(typeof(IngredientDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IngredientDetailResponse>> Create([FromBody] CreateIngredientRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _ingredientService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { uuid = created.Uuid }, created);
    }


    /// <summary>
    /// Update an existing ingredient
    /// </summary>
    /// <remarks>
    /// Updates the editable ingredient fields. Use the clear flags to explicitly null nullable fields in a PATCH request.
    /// </remarks>
    /// <param name="uuid">Ingredient UUID</param>
    /// <param name="request">Updated ingredient payload</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPatch("{uuid:guid}")]
    [ProducesResponseType(typeof(IngredientDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<IngredientDetailResponse>> Update(Guid uuid, [FromBody] UpdateIngredientRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _ingredientService.UpdateAsync(uuid, request, ct);
        return Ok(updated);
    }


    /// <summary>
    /// Get an ingredient by UUID
    /// </summary>
    /// <remarks>
    /// Returns the full ingredient details together with ingredient type and default unit labels.
    /// </remarks>
    /// <param name="uuid">Ingredient UUID</param>
    [HttpGet("{uuid:guid}")]
    [ProducesResponseType(typeof(IngredientDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IngredientDetailResponse>> GetById(Guid uuid, CancellationToken ct)
    {
        var ingredient = await _ingredientService.GetByIdAsync(uuid, ct);
        return ingredient is null ? NotFound() : Ok(ingredient);
    }


    /// <summary>
    /// Get all ingredients
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of ingredients with optional filters by ingredient type, active status, or free-text search.
    /// </remarks>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 12)</param>
    /// <param name="ingredientTypeUuid">Filter by ingredient type UUID (optional)</param>
    /// <param name="isActive">Filter by active status (optional)</param>
    /// <param name="search">Search on ingredient name (optional)</param>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<IngredientDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<IngredientDetailResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] Guid? ingredientTypeUuid = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _ingredientService.ListAsync(page, pageSize, ingredientTypeUuid, isActive, search, ct);
        return Ok(result);
    }


    /// <summary>
    /// Toggle ingredient active status
    /// </summary>
    /// <remarks>
    /// Flips the IsActive flag for the target ingredient.
    /// </remarks>
    /// <param name="uuid">Ingredient UUID</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPatch("{uuid:guid}/toggle-active")]
    [ProducesResponseType(typeof(IngredientDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<IngredientDetailResponse>> ToggleActive(Guid uuid, CancellationToken ct)
    {
        var ingredient = await _ingredientService.ToggleActiveAsync(uuid, ct);
        return Ok(ingredient);
    }


    /// <summary>
    /// Delete an ingredient
    /// </summary>
    /// <remarks>
    /// Deletes the ingredient when it is not referenced by recipes. Linked ingredient images are removed as part of the delete.
    /// </remarks>
    /// <param name="uuid">Ingredient UUID</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpDelete("{uuid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid uuid, CancellationToken ct)
    {
        await _ingredientService.DeleteAsync(uuid, ct);
        return NoContent();
    }

}
