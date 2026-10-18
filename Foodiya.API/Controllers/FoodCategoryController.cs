using Foodiya.API.Controllers.Common;
using Foodiya.Domain.Constants;
using Foodiya.Application.DTOs.FoodCategory.Request;
using Foodiya.Application.DTOs.FoodCategory.Response;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foodiya.API.Controllers;

public sealed class FoodCategoryController : BaseController
{
    private readonly IFoodCategoryService _foodCategoryService;

    public FoodCategoryController(IFoodCategoryService foodCategoryService)
    {
        _foodCategoryService = foodCategoryService ?? throw new FoodiyaNullArgumentException(nameof(foodCategoryService));
    }

    /// <summary>
    /// Create a new food category
    /// </summary>
    /// <remarks>
    /// Creates a new food category entry. Name and code should be unique.
    /// </remarks>
    /// <param name="request">Food category payload</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPost]
    [ProducesResponseType(typeof(FoodCategoryDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FoodCategoryDetailResponse>> Create([FromBody] CreateFoodCategoryRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _foodCategoryService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { uuid = created.Uuid }, created);
    }


    /// <summary>
    /// Update an existing food category
    /// </summary>
    /// <remarks>
    /// Updates the editable food category fields. Optional text fields can be cleared by sending an empty string.
    /// </remarks>
    /// <param name="uuid">Food category UUID</param>
    /// <param name="request">Updated food category payload</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPatch("{uuid:guid}")]
    [ProducesResponseType(typeof(FoodCategoryDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<FoodCategoryDetailResponse>> Update(Guid uuid, [FromBody] UpdateFoodCategoryRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _foodCategoryService.UpdateAsync(uuid, request, ct);
        return Ok(updated);
    }


    /// <summary>
    /// Get a food category by UUID
    /// </summary>
    /// <remarks>
    /// Returns the full food category details.
    /// </remarks>
    /// <param name="uuid">Food category UUID</param>
    [HttpGet("{uuid:guid}")]
    [ProducesResponseType(typeof(FoodCategoryDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FoodCategoryDetailResponse>> GetById(Guid uuid, CancellationToken ct)
    {
        var foodCategory = await _foodCategoryService.GetByIdAsync(uuid, ct);
        return foodCategory is null ? NotFound() : Ok(foodCategory);
    }


    /// <summary>
    /// Get all food categories
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of food categories with optional filters by active status or free-text search.
    /// </remarks>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 12)</param>
    /// <param name="isActive">Filter by active status (optional)</param>
    /// <param name="search">Search on category name, description, or code (optional)</param>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<FoodCategoryDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<FoodCategoryDetailResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _foodCategoryService.ListAsync(page, pageSize, isActive, search, ct);
        return Ok(result);
    }


    /// <summary>
    /// Toggle food category active status
    /// </summary>
    /// <remarks>
    /// Flips the IsActive flag for the target food category.
    /// </remarks>
    /// <param name="uuid">Food category UUID</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPatch("{uuid:guid}/toggle-active")]
    [ProducesResponseType(typeof(FoodCategoryDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<FoodCategoryDetailResponse>> ToggleActive(Guid uuid, CancellationToken ct)
    {
        var foodCategory = await _foodCategoryService.ToggleActiveAsync(uuid, ct);
        return Ok(foodCategory);
    }


    /// <summary>
    /// Delete a food category
    /// </summary>
    /// <remarks>
    /// Deletes the food category when it is not referenced by recipes.
    /// </remarks>
    /// <param name="uuid">Food category UUID</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpDelete("{uuid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid uuid, CancellationToken ct)
    {
        await _foodCategoryService.DeleteAsync(uuid, ct);
        return NoContent();
    }

}
