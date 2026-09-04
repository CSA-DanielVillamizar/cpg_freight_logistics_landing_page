using CPG.Application.Features.Authentication;
using CPG.Application.Features.Authentication.Login;
using CPG.Application.Features.Authentication.Refresh;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CPG.Api.Controllers;

/// <summary>Authentication and token lifecycle (SPEC.md US-01).</summary>
[AllowAnonymous]
public sealed class AuthController(ISender sender) : ApiControllerBase
{
    /// <summary>Exchange credentials for a JWT access token and a refresh token.</summary>
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
}
