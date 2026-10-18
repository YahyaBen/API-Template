using Foodiya.API.Controllers.Common;
using Foodiya.Domain.Constants;
using Foodiya.Application.DTOs.PresetAvatarImage.Request;
using Foodiya.Application.DTOs.PresetAvatarImage.Response;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foodiya.API.Controllers;

public sealed class PresetAvatarImageController : BaseController
{
    private readonly IPresetAvatarImageService _presetAvatarImageService;

    public PresetAvatarImageController(IPresetAvatarImageService presetAvatarImageService)
    {
        _presetAvatarImageService = presetAvatarImageService ?? throw new FoodiyaNullArgumentException(nameof(presetAvatarImageService));
    }

    /// <summary>
    /// Create a preset avatar image
    /// </summary>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPost]
    [ProducesResponseType(typeof(PresetAvatarImageDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PresetAvatarImageDetailResponse>> Create([FromBody] CreatePresetAvatarImageRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _presetAvatarImageService.CreateAsync(request, ct);
        return CreatedAtAction(nameof(GetById), new { uuid = created.Uuid }, created);
    }


    /// <summary>
    /// Update a preset avatar image
    /// </summary>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPatch("{uuid:guid}")]
    [ProducesResponseType(typeof(PresetAvatarImageDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<PresetAvatarImageDetailResponse>> Update(Guid uuid, [FromBody] UpdatePresetAvatarImageRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _presetAvatarImageService.UpdateAsync(uuid, request, ct);
        return Ok(updated);
    }


    /// <summary>
    /// Get a preset avatar image by UUID
    /// </summary>
    [HttpGet("{uuid:guid}")]
    [ProducesResponseType(typeof(PresetAvatarImageDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PresetAvatarImageDetailResponse>> GetById(Guid uuid, CancellationToken ct)
    {
        var presetAvatarImage = await _presetAvatarImageService.GetByIdAsync(uuid, ct);
        return presetAvatarImage is null ? NotFound() : Ok(presetAvatarImage);
    }


    /// <summary>
    /// Get all preset avatar images
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of preset avatar images with optional filters by active status or free-text search.
    /// </remarks>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<PresetAvatarImageDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<PresetAvatarImageDetailResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _presetAvatarImageService.ListAsync(page, pageSize, isActive, search, ct);
        return Ok(result);
    }


    /// <summary>
    /// Toggle preset avatar image active status
    /// </summary>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPatch("{uuid:guid}/toggle-active")]
    [ProducesResponseType(typeof(PresetAvatarImageDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PresetAvatarImageDetailResponse>> ToggleActive(Guid uuid, CancellationToken ct)
    {
        var updated = await _presetAvatarImageService.ToggleActiveAsync(uuid, ct);
        return Ok(updated);
    }


    /// <summary>
    /// Delete a preset avatar image
    /// </summary>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpDelete("{uuid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid uuid, CancellationToken ct)
    {
        await _presetAvatarImageService.DeleteAsync(uuid, ct);
        return NoContent();
    }

}
