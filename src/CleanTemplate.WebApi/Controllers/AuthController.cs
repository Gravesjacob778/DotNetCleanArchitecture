using CleanTemplate.Application.Common.Interfaces;
using CleanTemplate.WebApi.Models.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CleanTemplate.WebApi.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IIdentityService identityService) : ControllerBase
{
    private readonly IIdentityService _identityService = identityService;

    [AllowAnonymous]
    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register(RegisterRequest request, CancellationToken cancellationToken)
    {
        var result = await _identityService.RegisterAsync(request.Email, request.Password, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(new AuthResponse(null, result.Errors));
        }

        return Ok(new AuthResponse(result.Token, []));
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login(LoginRequest request, CancellationToken cancellationToken)
    {
        var result = await _identityService.LoginAsync(request.Email, request.Password, cancellationToken);
        if (!result.Succeeded)
        {
            return Unauthorized(new AuthResponse(null, result.Errors));
        }

        return Ok(new AuthResponse(result.Token, Array.Empty<string>()));
    }
}
