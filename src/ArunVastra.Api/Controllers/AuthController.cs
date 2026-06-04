using System.Security.Claims;
using ArunVastra.Application.DTOs.Auth;
using ArunVastra.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace ArunVastra.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
[SwaggerTag("Authentication endpoints for login, refresh token rotation, and logout.")]
public sealed class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("login")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Login",
        Description = "Authenticates a user with email and password, migrates legacy password when required, and returns access and refresh tokens.")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.LoginAsync(request, cancellationToken);

        if (!response.Success)
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }

    [HttpPost("refresh")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Refresh access token",
        Description = "Validates an active refresh token, revokes it, and returns a new access token and refresh token.")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LoginResponse>> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.RefreshAsync(request, cancellationToken);

        if (!response.Success)
        {
            return Unauthorized(response);
        }

        return Ok(response);
    }

    [HttpPost("logout")]
    [Consumes("application/json")]
    [SwaggerOperation(
        Summary = "Logout current session",
        Description = "Revokes the supplied refresh token for the current browser/device session.")]
    [ProducesResponseType(typeof(AuthActionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthActionResponse), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthActionResponse>> Logout(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        var response = await _authService.LogoutAsync(request, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    [Authorize]
    [HttpPost("logout-all")]
    [SwaggerOperation(
        Summary = "Logout all sessions",
        Description = "Revokes all active refresh tokens for the authenticated user.")]
    [ProducesResponseType(typeof(AuthActionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthActionResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(AuthActionResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthActionResponse>> LogoutAll(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            return Unauthorized(new AuthActionResponse
            {
                Success = false,
                Message = "Invalid access token."
            });
        }

        var response = await _authService.LogoutAllAsync(userId, cancellationToken);

        if (!response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }
}
