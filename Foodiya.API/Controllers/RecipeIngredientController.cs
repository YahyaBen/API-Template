using Foodiya.API.Controllers.Common;
using Foodiya.Domain.Constants;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Application.DTOs.RecipeIngredient.Request;
using Foodiya.Application.DTOs.RecipeIngredient.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foodiya.API.Controllers;

public sealed class RecipeIngredientController : BaseController
{
    private readonly IRecipeIngredientService _recipeIngredientService;

    public RecipeIngredientController(IRecipeIngredientService recipeIngredientService)
    {
        _recipeIngredientService = recipeIngredientService ?? throw new FoodiyaNullArgumentException(nameof(recipeIngredientService));
    }

    /// <summary>
    /// Create a recipe ingredient
    /// </summary>
    [Authorize(Roles = AppRoleConstants.ChefOrAbove)]
    [HttpPost]
    [ProducesResponseType(typeof(RecipeIngredientDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RecipeIngredientDetailResponse>> Create([FromBody] CreateRecipeIngredientRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _recipeIngredientService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { uuid = created.Uuid }, created);
    }


    /// <summary>
    /// Update a recipe ingredient
    /// </summary>
    [Authorize(Roles = AppRoleConstants.ChefOrAbove)]
    [HttpPatch("{uuid:guid}")]
    [ProducesResponseType(typeof(RecipeIngredientDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeIngredientDetailResponse>> Update(
        Guid uuid,
        [FromBody] UpdateRecipeIngredientRequest request,
        CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _recipeIngredientService.UpdateAsync(uuid, request, ct);
        return Ok(updated);
    }


    /// <summary>
    /// Get a recipe ingredient by composite key
    /// </summary>
    [HttpGet("{uuid:guid}")]
    [ProducesResponseType(typeof(RecipeIngredientDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<RecipeIngredientDetailResponse>> GetById(Guid uuid, CancellationToken ct)
    {
        var recipeIngredient = await _recipeIngredientService.GetByIdAsync(uuid, ct);
        return recipeIngredient is null ? NotFound() : Ok(recipeIngredient);
    }


    /// <summary>
    /// Get all recipe ingredients
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of recipe ingredients with optional filters by recipe, ingredient, unit, or free-text search.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<RecipeIngredientDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<RecipeIngredientDetailResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] Guid? recipeUuid = null,
        [FromQuery] Guid? ingredientUuid = null,
        [FromQuery] Guid? unitUuid = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _recipeIngredientService.ListAsync(page, pageSize, recipeUuid, ingredientUuid, unitUuid, search, ct);
        return Ok(result);
    }


    /// <summary>
    /// Delete a recipe ingredient
    /// </summary>
    [Authorize(Roles = AppRoleConstants.ChefOrAbove)]
    [HttpDelete("{uuid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid uuid, CancellationToken ct)
    {
        await _recipeIngredientService.DeleteAsync(uuid, ct);
        return NoContent();
    }

}
