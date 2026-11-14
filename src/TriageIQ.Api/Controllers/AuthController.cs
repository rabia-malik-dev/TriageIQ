using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TriageIQ.Application.Auth;
using TriageIQ.Application.Validation;

namespace TriageIQ.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(AuthService authService, ValidationService validation) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        await validation.ValidateAndThrowAsync(request, cancellationToken);
        var response = await authService.RegisterAsync(request, cancellationToken);
        return Ok(response);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<AuthResponse>> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        await validation.ValidateAndThrowAsync(request, cancellationToken);
        var response = await authService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }
}