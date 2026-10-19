using Foodiya.API.Controllers.Common;
using Foodiya.Domain.Constants;
using Foodiya.Application.DTOs.ChefProfile.Request;
using Foodiya.Application.DTOs.ChefProfile.Response;
using Foodiya.Application.DTOs.Common;
using Foodiya.Application.DTOs.Recipe.Response;
using Foodiya.Domain.Exceptions;
using Foodiya.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;

namespace Foodiya.API.Controllers;

public sealed class ChefProfileController : BaseController
{
    private readonly IChefProfileService _chefProfileService;

    public ChefProfileController(IChefProfileService chefProfileService)
    {
        _chefProfileService = chefProfileService ?? throw new FoodiyaNullArgumentException(nameof(chefProfileService));
    }

    /// <summary>
    /// Create a new chef profile
    /// </summary>
    /// <remarks>
    /// Creates a chef profile for the specified AppUser. One user can own only one chef profile.
    /// </remarks>
    /// <param name="userUuid">AppUser UUID that will own the profile</param>
    /// <param name="request">Chef profile payload</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPost("{userUuid:guid}")]
    [ProducesResponseType(typeof(ChefProfileDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ChefProfileDetailResponse>> Create(Guid userUuid, [FromBody] CreateChefProfileRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _chefProfileService.CreateAsync(userUuid, request, ct);
        return CreatedAtAction(nameof(GetById), new { uuid = created.Uuid }, created);
    }


    /// <summary>
    /// Self-promote: create your own chef profile
    /// </summary>
    /// <remarks>
    /// Allows any authenticated user to create their own chef profile without admin intervention.
    /// Returns 409 if a chef profile already exists for the current user.
    /// </remarks>
    /// <param name="request">Chef profile payload</param>
    [Authorize(Roles = AppRoleConstants.All)]
    [HttpPost("me")]
    [ProducesResponseType(typeof(ChefProfileDetailResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ChefProfileDetailResponse>> CreateSelf([FromBody] CreateChefProfileRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        if (!Guid.TryParse(User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value, out var identityUserId))
            return Unauthorized();

        var created = await _chefProfileService.CreateSelfAsync(identityUserId, request, ct);
        return CreatedAtAction(nameof(GetById), new { uuid = created.Uuid }, created);
    }


    /// <summary>
    /// Update an existing chef profile
    /// </summary>
    /// <remarks>
    /// Updates the editable chef profile fields. Optional text fields can be cleared by sending an empty string.
    /// </remarks>
    /// <param name="uuid">Chef profile UUID</param>
    /// <param name="request">Updated chef profile payload</param>
    [Authorize(Roles = AppRoleConstants.AdminOrAbove)]
    [HttpPatch("{uuid:guid}")]
    [ProducesResponseType(typeof(ChefProfileDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChefProfileDetailResponse>> Update(Guid uuid, [FromBody] UpdateChefProfileRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _chefProfileService.UpdateAsync(uuid, request, ct);
        return Ok(updated);
    }


    /// <summary>
    /// Get a chef profile by UUID
    /// </summary>
    /// <remarks>
    /// Returns the chef profile details together with the linked AppUser identity fields.
    /// </remarks>
    /// <param name="uuid">Chef profile UUID</param>
    [HttpGet("{uuid:guid}")]
    [ProducesResponseType(typeof(ChefProfileDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChefProfileDetailResponse>> GetById(Guid uuid, CancellationToken ct)
    {
        var chefProfile = await _chefProfileService.GetByIdAsync(uuid, ct);
        return chefProfile is null ? NotFound() : Ok(chefProfile);
    }


    /// <summary>
    /// Get all chef profiles
    /// </summary>
    /// <remarks>
    /// Returns a paginated list of chef profiles with optional filters by verification status or free-text search.
    /// </remarks>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="pageSize">Page size (default: 12)</param>
    /// <param name="isVerified">Filter by verification status (optional)</param>
    /// <param name="search">Search on display name, bio, specialty, or user identity fields (optional)</param>
    [HttpGet]
    [ProducesResponseType(typeof(PaginatedResponse<ChefProfileDetailResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<PaginatedResponse<ChefProfileDetailResponse>>> List(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 12,
        [FromQuery] bool? isVerified = null,
        [FromQuery] string? search = null,
        CancellationToken ct = default)
    {
        var result = await _chefProfileService.ListAsync(page, pageSize, isVerified, search, ct);
        return Ok(result);
    }


    /// <summary>
    /// Toggle chef verification status
    /// </summary>
    /// <remarks>
    /// Flips the IsVerified flag for the target chef profile.
    /// </remarks>
    /// <param name="uuid">Chef profile UUID</param>
    [Authorize(Roles = AppRoleConstants.SuperAdminOnly)]
    [HttpPatch("{uuid:guid}/toggle-verified")]
    [ProducesResponseType(typeof(ChefProfileDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ChefProfileDetailResponse>> ToggleVerified(Guid uuid, CancellationToken ct)
    {
        var chefProfile = await _chefProfileService.ToggleVerifiedAsync(uuid, ct);
        return Ok(chefProfile);
    }


    /// <summary>
    /// Delete a chef profile (soft delete)
    /// </summary>
    /// <remarks>
    /// Marks the chef profile as deleted without removing data.
    /// Requires the user performing the action and a mandatory reason.
    /// Cannot delete a profile that still owns active recipes.
    /// </remarks>
    /// <param name="uuid">Chef profile UUID</param>
    /// <param name="request">User and reason for the deletion</param>
    [Authorize(Roles = AppRoleConstants.SuperAdminOnly)]
    [HttpDelete("{uuid:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(Guid uuid, [FromBody] SoftDeleteRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        await _chefProfileService.DeleteAsync(uuid, request, ct);
        return NoContent();
    }

}
