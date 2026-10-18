using Foodiya.API.Controllers.Common;
using Foodiya.Domain.Constants;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Application.DTOs.RecipeStep.Request;
using Foodiya.Application.DTOs.RecipeStep.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foodiya.API.Controllers;

public sealed class RecipeStepController : BaseController
{
    private readonly IRecipeStepService _recipeStepService;

    public RecipeStepController(IRecipeStepService recipeStepService)
    {
        _recipeStepService = recipeStepService ?? throw new FoodiyaNullArgumentException(nameof(recipeStepService));
    }

    /// <summary>
    /// Create a recipe step
    /// </summary>
    /// <remarks>
    /// Creates a new step under the specified recipe.
    /// </remarks>
    /// <param name="request">Recipe step payload</param>
    [Authorize(Roles = AppRoleConstants.ChefOrAbove)]
    [HttpPost]
    [ProducesResponseType(typeof(RecipeStepDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RecipeStepDetailResponse>> Create([FromBody] CreateRecipeStepItemRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _recipeStepService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { uuid = created.Uuid }, created);
    }


    /// <summary>
    /// Update a recipe step
    /// </summary>
    /// <remarks>
    /// Updates the editable step fields. Duration can be cleared with the explicit clear flag.
    /// </remarks>
    /// <param name="uuid">Recipe step UUID</param>
    /// <param name="request">Updated recipe step payload</param>
    [Authorize(Roles = AppRoleConstants.ChefOrAbove)]
    [HttpPatch("{uuid:guid}")]
    [ProducesResponseType(typeof(RecipeStepDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RecipeStepDetailResponse>> Update(Guid uuid, [FromBody] UpdateRecipeStepItemRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _recipeStepService.UpdateAsync(uuid, request, ct);
        return Ok(updated);
    }


    /// <summary>
    /// Get a recipe step by UUID
    /// </summary>
    /// <remarks>
    /// Returns the full recipe step details.
    /// </remarks>
    /// <param name="uuid">Recipe step UUID</param>
    [HttpGet("{uuid:guid}")]
    [ProducesResponseType(typeof(RecipeStepDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeStepDetailResponse>> GetById(Guid uuid, CancellationToken ct)
    {
        var recipeStep = await _recipeStepService.GetByIdAsync(uuid, ct);
        return recipeStep is null ? NotFound() : Ok(recipeStep);
    }


    /// <summary>
    /// Get all recipe steps
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of recipe steps with optional filtering by recipe or free-text search.
    /// </remarks>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 12)</param>
    /// <param name="recipeUuid">Filter by recipe UUID (optional)</param>
    /// <param name="search">Search on recipe title, step title, or instruction (optional)</param>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<RecipeStepDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<RecipeStepDetailResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] Guid? recipeUuid = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _recipeStepService.ListAsync(page, pageSize, recipeUuid, search, ct);
        return Ok(result);
    }


    /// <summary>
    /// Delete a recipe step
    /// </summary>
    /// <remarks>
    /// Deletes the specified recipe step record.
    /// </remarks>
    /// <param name="uuid">Recipe step UUID</param>
    [Authorize(Roles = AppRoleConstants.ChefOrAbove)]
    [HttpDelete("{uuid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid uuid, CancellationToken ct)
    {
        await _recipeStepService.DeleteAsync(uuid, ct);
        return NoContent();
    }

}
