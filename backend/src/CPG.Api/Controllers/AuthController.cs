using CPG.Application.Features.Authentication;
using CPG.Application.Features.Authentication.Login;
using CPG.Application.Features.Authentication.Me;
using CPG.Application.Features.Authentication.Refresh;
using CPG.Application.Features.Authentication.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CPG.Api.Controllers;

/// <summary>Authentication and token lifecycle (SPEC.md US-01, T-SDD Epica 1 Tri-Sign-Up).</summary>
public sealed class AuthController(ISender sender) : ApiControllerBase
{
    /// <summary>Exchange credentials for a JWT access token and a refresh token.</summary>
    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(
            new LoginCommand(request.Email, request.Password), cancellationToken);
        return Ok(response);
    }

    /// <summary>Exchange a valid refresh token for a new access token.</summary>
    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Refresh(
        [FromBody] RefreshRequest request,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(
            new RefreshTokenCommand(request.RefreshToken), cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Tri-Sign-Up: creates the account (Shipper, Carrier or Agent), the matching business
    /// profile, and signs the visitor in immediately. Returns 409 if the email is already taken.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(RegisterUserCommand.FromRequest(request), cancellationToken);
        return Created("/api/me", response);
    }

    /// <summary>The authenticated principal plus their role-specific profile.</summary>
    [Authorize]
    [HttpGet("/api/me")]
    [ProducesResponseType(typeof(MyProfileResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<MyProfileResponse>> Me(CancellationToken cancellationToken)
    {
        var response = await sender.Send(new GetMyProfileQuery(), cancellationToken);
        return Ok(response);
    }
}
