using Foodiya.API.Controllers.Common;
using Foodiya.Domain.Constants;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Application.DTOs.RecipeImage.Request;
using Foodiya.Application.DTOs.RecipeImage.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foodiya.API.Controllers;

public sealed class RecipeImageController : BaseController
{
    private readonly IRecipeImageService _recipeImageService;

    public RecipeImageController(IRecipeImageService recipeImageService)
    {
        _recipeImageService = recipeImageService ?? throw new FoodiyaNullArgumentException(nameof(recipeImageService));
    }

    /// <summary>
    /// Create a recipe image
    /// </summary>
    [Authorize(Roles = AppRoleConstants.ChefOrAbove)]
    [HttpPost]
    [ProducesResponseType(typeof(RecipeImageDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeImageDetailResponse>> Create([FromBody] CreateRecipeImageRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _recipeImageService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { uuid = created.Uuid }, created);
    }


    /// <summary>
    /// Update a recipe image
    /// </summary>
    [Authorize(Roles = AppRoleConstants.ChefOrAbove)]
    [HttpPatch("{uuid:guid}")]
    [ProducesResponseType(typeof(RecipeImageDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeImageDetailResponse>> Update(Guid uuid, [FromBody] UpdateRecipeImageRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _recipeImageService.UpdateAsync(uuid, request, ct);
        return Ok(updated);
    }


    /// <summary>
    /// Get a recipe image by UUID
    /// </summary>
    [HttpGet("{uuid:guid}")]
    [ProducesResponseType(typeof(RecipeImageDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeImageDetailResponse>> GetById(Guid uuid, CancellationToken ct)
    {
        var recipeImage = await _recipeImageService.GetByIdAsync(uuid, ct);
        return recipeImage is null ? NotFound() : Ok(recipeImage);
    }


    /// <summary>
    /// Get all recipe images
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of recipe images with optional filters by recipe, primary flag, or free-text search.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<RecipeImageDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<RecipeImageDetailResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] Guid? recipeUuid = null,
        [FromQuery] bool? isPrimary = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _recipeImageService.ListAsync(page, pageSize, recipeUuid, isPrimary, search, ct);
        return Ok(result);
    }


    /// <summary>
    /// Delete a recipe image
    /// </summary>
    [Authorize(Roles = AppRoleConstants.ChefOrAbove)]
    [HttpDelete("{uuid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid uuid, CancellationToken ct)
    {
        await _recipeImageService.DeleteAsync(uuid, ct);
        return NoContent();
    }

}
