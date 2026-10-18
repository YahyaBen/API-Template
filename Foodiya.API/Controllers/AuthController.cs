using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Foodiya.API.Controllers.Common;
using Foodiya.Application.DTOs.Auth.Request;
using Foodiya.Application.DTOs.Auth.Response;
using Foodiya.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Foodiya.API.Controllers;

public sealed class AuthController : BaseController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Register a local account
    /// </summary>
    /// <remarks>
    /// Creates a new AppUser using email and password credentials, then returns the API JWT.
    /// </remarks>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthTokenResponse>> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _authService.RegisterAsync(request, ct);
        return CreatedAtAction(nameof(Me), response);
    }

    /// <summary>
    /// Sign in with email or username and password
    /// </summary>
    /// <remarks>
    /// Validates local credentials against AppUser.PasswordHash and PasswordSalt, then returns the API JWT.
    /// </remarks>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthTokenResponse>> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _authService.LoginAsync(request, ct);
        return Ok(response);
    }

    /// <summary>
    /// Sign in with Google
    /// </summary>
    /// <remarks>
    /// Validates a Google ID token on the backend, links or creates the internal user, and returns the API JWT.
    /// </remarks>
    [AllowAnonymous]
    [HttpPost("google")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthTokenResponse>> Google([FromBody] GoogleSignInRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _authService.SignInWithGoogleAsync(request, ct);
        return Ok(response);
    }

    /// <summary>
    /// Refresh an expired access token
    /// </summary>
    /// <remarks>
    /// Accepts an expired access token and a valid refresh token.
    /// Returns a new access token and a rotated refresh token (the old one is revoked).
    /// </remarks>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthTokenResponse>> Refresh([FromBody] RefreshTokenRequest request, CancellationToken ct)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var response = await _authService.RefreshAsync(request, ct);
        return Ok(response);
    }

    /// <summary>
    /// Get the current authenticated user
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(AuthUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AuthUserResponse>> Me(CancellationToken ct)
    {
        var subClaim = User.FindFirstValue(JwtRegisteredClaimNames.Sub);
        if (!int.TryParse(subClaim, out var userId))
            return Unauthorized();

        var user = await _authService.GetCurrentUserAsync(userId, ct);
        return user is null ? NotFound() : Ok(user);
    }
}
