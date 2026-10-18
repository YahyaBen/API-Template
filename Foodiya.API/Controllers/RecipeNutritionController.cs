using Foodiya.API.Controllers.Common;
using Foodiya.Domain.Constants;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Application.DTOs.RecipeNutrition.Request;
using Foodiya.Application.DTOs.RecipeNutrition.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foodiya.API.Controllers;

public sealed class RecipeNutritionController : BaseController
{
    private readonly IRecipeNutritionService _recipeNutritionService;

    public RecipeNutritionController(IRecipeNutritionService recipeNutritionService)
    {
        _recipeNutritionService = recipeNutritionService ?? throw new FoodiyaNullArgumentException(nameof(recipeNutritionService));
    }

    /// <summary>
    /// Create recipe nutrition
    /// </summary>
    /// <remarks>
    /// Creates the one-to-one nutrition row for a recipe. A recipe can only have one nutrition record.
    /// </remarks>
    /// <param name="request">Recipe nutrition payload</param>
    [Authorize(Roles = AppRoleConstants.ChefOrAbove)]
    [HttpPost]
    [ProducesResponseType(typeof(RecipeNutritionDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RecipeNutritionDetailResponse>> Create([FromBody] CreateRecipeNutritionRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _recipeNutritionService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { recipeUuid = created.RecipeUuid }, created);
    }


    /// <summary>
    /// Update recipe nutrition
    /// </summary>
    /// <remarks>
    /// Updates the nutrition values for the target recipe. Nullable macro fields can be cleared with the explicit clear flags.
    /// </remarks>
    /// <param name="recipeUuid">Recipe UUID</param>
    /// <param name="request">Updated recipe nutrition payload</param>
    [Authorize(Roles = AppRoleConstants.ChefOrAbove)]
    [HttpPatch("{recipeUuid:guid}")]
    [ProducesResponseType(typeof(RecipeNutritionDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeNutritionDetailResponse>> Update(Guid recipeUuid, [FromBody] UpdateRecipeNutritionRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _recipeNutritionService.UpdateAsync(recipeUuid, request, ct);
        return Ok(updated);
    }


    /// <summary>
    /// Get recipe nutrition by recipe UUID
    /// </summary>
    /// <remarks>
    /// Returns the nutrition row linked to the specified recipe.
    /// </remarks>
    /// <param name="recipeUuid">Recipe UUID</param>
    [HttpGet("{recipeUuid:guid}")]
    [ProducesResponseType(typeof(RecipeNutritionDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeNutritionDetailResponse>> GetById(Guid recipeUuid, CancellationToken ct)
    {
        var recipeNutrition = await _recipeNutritionService.GetByIdAsync(recipeUuid, ct);
        return recipeNutrition is null ? NotFound() : Ok(recipeNutrition);
    }


    /// <summary>
    /// Get all recipe nutrition rows
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of recipe nutrition records with optional filters by recipe or free-text search on recipe title.
    /// </remarks>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 12)</param>
    /// <param name="recipeUuid">Filter by recipe UUID (optional)</param>
    /// <param name="search">Search on recipe title (optional)</param>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<RecipeNutritionDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<RecipeNutritionDetailResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] Guid? recipeUuid = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _recipeNutritionService.ListAsync(page, pageSize, recipeUuid, search, ct);
        return Ok(result);
    }


    /// <summary>
    /// Delete recipe nutrition
    /// </summary>
    /// <remarks>
    /// Deletes the nutrition row linked to the specified recipe.
    /// </remarks>
    /// <param name="recipeUuid">Recipe UUID</param>
    [Authorize(Roles = AppRoleConstants.ChefOrAbove)]
    [HttpDelete("{recipeUuid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid recipeUuid, CancellationToken ct)
    {
        await _recipeNutritionService.DeleteAsync(recipeUuid, ct);
        return NoContent();
    }

}
