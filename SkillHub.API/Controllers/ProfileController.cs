using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Features.Profile.DTOs;
using SkillHub.Application.Features.Profile.Interfaces;

namespace SkillHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProfileController : ControllerBase
{
    private readonly IProfileService _profileService;

    public ProfileController(IProfileService profileService)
    {
        _profileService = profileService;
    }

    private long? GetUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    [HttpGet]
    public async Task<IActionResult> GetProfile()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized("Invalid token claim");

        var profile = await _profileService.GetProfileAsync(userId.Value);
        if (profile == null) return NotFound("User profile not found");

        return Ok(profile);
    }

    [HttpPut]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized("Invalid token claim");

        if (string.IsNullOrWhiteSpace(dto.FirstName) || string.IsNullOrWhiteSpace(dto.LastName))
            return BadRequest("First Name and Last Name are required");

        var success = await _profileService.UpdateProfileAsync(userId.Value, dto);
        if (!success) return NotFound("User not found");

        return Ok(new { message = "Profile updated successfully" });
    }

    [HttpPut("password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto dto)
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized("Invalid token claim");

        if (string.IsNullOrWhiteSpace(dto.CurrentPassword) || string.IsNullOrWhiteSpace(dto.NewPassword))
            return BadRequest("Current password and new password are required");

        var (success, message) = await _profileService.ChangePasswordAsync(userId.Value, dto);
        if (!success) return BadRequest(new { message });

        return Ok(new { message });
    }
}
