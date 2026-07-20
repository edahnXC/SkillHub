using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Features.Identity.DTOs;
using SkillHub.Application.Features.Identity.Interfaces;

namespace SkillHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(
        IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]  //when agular sends a request to api/auth/register, this method will be called
    public async Task<IActionResult> Register(
        RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }
        catch (Exception ex) when (ex.Message == "Invalid credentials." || ex.Message == "Account is deactivated.")
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        RefreshTokenRequestDto request)
    {
        var result = await _authService
            .RefreshTokenAsync(request.Token);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(
        RefreshTokenRequestDto request)
    {
        await _authService.LogoutAsync(request.Token);
        return Ok(new { message = "Logged out successfully." });
    }

}