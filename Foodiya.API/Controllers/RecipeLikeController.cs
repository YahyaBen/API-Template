using Foodiya.API.Controllers.Common;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Application.DTOs.RecipeLike.Request;
using Foodiya.Application.DTOs.RecipeLike.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace Foodiya.API.Controllers;

public sealed class RecipeLikeController : BaseController
{
    private readonly IRecipeLikeService _recipeLikeService;

    public RecipeLikeController(IRecipeLikeService recipeLikeService)
    {
        _recipeLikeService = recipeLikeService ?? throw new FoodiyaNullArgumentException(nameof(recipeLikeService));
    }

    /// <summary>
    /// Create a recipe like
    /// </summary>
    /// <remarks>
    /// Creates a like relation between a user and a recipe.
    /// </remarks>
    /// <param name="request">Recipe like payload</param>
    [HttpPost]
    [ProducesResponseType(typeof(RecipeLikeDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RecipeLikeDetailResponse>> Create([FromBody] CreateRecipeLikeRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _recipeLikeService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { recipeUuid = created.RecipeUuid, userUuid = created.UserUuid }, created);
    }


    /// <summary>
    /// Get a recipe like by composite key
    /// </summary>
    /// <remarks>
    /// Returns the recipe like details for the given recipe and user pair.
    /// </remarks>
    /// <param name="recipeUuid">Recipe UUID</param>
    /// <param name="userUuid">User UUID</param>
    [HttpGet("{recipeUuid:guid}/{userUuid:guid}")]
    [ProducesResponseType(typeof(RecipeLikeDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeLikeDetailResponse>> GetById(Guid recipeUuid, Guid userUuid, CancellationToken ct)
    {
        var recipeLike = await _recipeLikeService.GetByIdAsync(recipeUuid, userUuid, ct);
        return recipeLike is null ? NotFound() : Ok(recipeLike);
    }


    /// <summary>
    /// Get all recipe likes
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of recipe likes with optional filters by recipe, user, or free-text search.
    /// </remarks>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 12)</param>
    /// <param name="recipeUuid">Filter by recipe UUID (optional)</param>
    /// <param name="userUuid">Filter by user UUID (optional)</param>
    /// <param name="search">Search on recipe title or user names (optional)</param>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<RecipeLikeDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<RecipeLikeDetailResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] Guid? recipeUuid = null,
        [FromQuery] Guid? userUuid = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _recipeLikeService.ListAsync(page, pageSize, recipeUuid, userUuid, search, ct);
        return Ok(result);
    }


    /// <summary>
    /// Delete a recipe like
    /// </summary>
    /// <remarks>
    /// Removes the like relation between a user and a recipe.
    /// </remarks>
    /// <param name="recipeUuid">Recipe UUID</param>
    /// <param name="userUuid">User UUID</param>
    [HttpDelete("{recipeUuid:guid}/{userUuid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid recipeUuid, Guid userUuid, CancellationToken ct)
    {
        await _recipeLikeService.DeleteAsync(recipeUuid, userUuid, ct);
        return NoContent();
    }

}
