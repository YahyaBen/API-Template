using Foodiya.API.Controllers.Common;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Application.DTOs.RecipeShare.Request;
using Foodiya.Application.DTOs.RecipeShare.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Foodiya.API.Controllers;

public sealed class RecipeShareController : BaseController
{
    private readonly IRecipeShareService _recipeShareService;

    public RecipeShareController(IRecipeShareService recipeShareService)
    {
        _recipeShareService = recipeShareService ?? throw new FoodiyaNullArgumentException(nameof(recipeShareService));
    }

    /// <summary>
    /// Create a recipe share
    /// </summary>
    /// <remarks>
    /// Creates a share event for a recipe. The recipient user is optional, depending on the share channel.
    /// </remarks>
    /// <param name="request">Recipe share payload</param>
    [HttpPost]
    [ProducesResponseType(typeof(RecipeShareDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeShareDetailResponse>> Create([FromBody] CreateRecipeShareRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _recipeShareService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { uuid = created.Uuid }, created);
    }


    /// <summary>
    /// Update a recipe share
    /// </summary>
    /// <remarks>
    /// Updates the editable share metadata. Nullable recipient and message fields can be cleared with explicit flags.
    /// </remarks>
    /// <param name="uuid">Recipe share UUID</param>
    /// <param name="request">Updated recipe share payload</param>
    [HttpPatch("{uuid:guid}")]
    [ProducesResponseType(typeof(RecipeShareDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeShareDetailResponse>> Update(Guid uuid, [FromBody] UpdateRecipeShareRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _recipeShareService.UpdateAsync(uuid, request, ct);
        return Ok(updated);
    }


    /// <summary>
    /// Get a recipe share by UUID
    /// </summary>
    /// <remarks>
    /// Returns the full recipe share details.
    /// </remarks>
    /// <param name="uuid">Recipe share UUID</param>
    [HttpGet("{uuid:guid}")]
    [ProducesResponseType(typeof(RecipeShareDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeShareDetailResponse>> GetById(Guid uuid, CancellationToken ct)
    {
        var recipeShare = await _recipeShareService.GetByIdAsync(uuid, ct);
        return recipeShare is null ? NotFound() : Ok(recipeShare);
    }


    /// <summary>
    /// Get all recipe shares
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of recipe shares with optional filters by recipe, sharer, recipient, channel, or free-text search.
    /// </remarks>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 12)</param>
    /// <param name="recipeUuid">Filter by recipe UUID (optional)</param>
    /// <param name="sharedByUserUuid">Filter by user who shared the recipe (optional)</param>
    /// <param name="sharedWithUserUuid">Filter by recipient user (optional)</param>
    /// <param name="channel">Filter by share channel (optional)</param>
    /// <param name="search">Search on recipe title, user names, channel, or message (optional)</param>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<RecipeShareDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<RecipeShareDetailResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] Guid? recipeUuid = null,
        [FromQuery] Guid? sharedByUserUuid = null,
        [FromQuery] Guid? sharedWithUserUuid = null,
        [FromQuery] string? channel = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _recipeShareService.ListAsync(page, pageSize, recipeUuid, sharedByUserUuid, sharedWithUserUuid, channel, search, ct);
        return Ok(result);
    }


    /// <summary>
    /// Delete a recipe share
    /// </summary>
    /// <remarks>
    /// Deletes the specified recipe share record.
    /// </remarks>
    /// <param name="uuid">Recipe share UUID</param>
    [HttpDelete("{uuid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid uuid, CancellationToken ct)
    {
        await _recipeShareService.DeleteAsync(uuid, ct);
        return NoContent();
    }

}
