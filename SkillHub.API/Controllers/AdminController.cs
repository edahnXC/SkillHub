using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Features.Admin.Interfaces;
using SkillHub.Application.Features.Admin.DTOs;

namespace SkillHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SuperAdmin,SubAdmin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetAllUsers()
    {
        var users = await _adminService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpDelete("users/{id}")]
    public async Task<IActionResult> DeleteUser(long id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var rolesClaim = User.FindAll(ClaimTypes.Role).Select(c => c.Value).ToList();

        if (!long.TryParse(userIdClaim, out var currentUserId))
        {
            return Unauthorized();
        }

        var targetUser = await _adminService.GetUserByIdAsync(id);
        if (targetUser == null)
        {
            return NotFound(new { message = "User not found." });
        }

        // Prevent deleting SuperAdmin accounts entirely
        if (targetUser.Roles.Contains("SuperAdmin"))
        {
            return BadRequest(new { message = "SuperAdmin accounts cannot be deleted." });
        }

        // Prevent SuperAdmin from deleting their own account
        if (rolesClaim.Contains("SuperAdmin") && currentUserId == id)
        {
            return BadRequest(new { message = "SuperAdmin cannot delete their own account." });
        }

        // SubAdmins cannot delete other administrators (SubAdmin or SuperAdmin)
        // Only SuperAdmin has the permission to delete SubAdmins
        bool isTargetAdmin = targetUser.Roles.Contains("SubAdmin") || targetUser.Roles.Contains("SuperAdmin");
        bool isCallerSuperAdmin = rolesClaim.Contains("SuperAdmin");

        if (isTargetAdmin && !isCallerSuperAdmin)
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { 
                message = "SubAdmins do not have permission to delete any administrator. Only the SuperAdmin can delete SubAdmins." 
            });
        }

        await _adminService.DeleteUserAsync(id);
        return NoContent();
    }

    [HttpPost("create-user")]
    [Authorize(Roles = "SuperAdmin")]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request)
    {
        try
        {
            await _adminService.CreateUserAsync(request);
            return Ok(new { message = "User created successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("courses")]
    public async Task<IActionResult> GetAllCourses()
    {
        var courses = await _adminService.GetAllCoursesAsync();
        return Ok(courses);
    }

    [HttpDelete("courses/{id}")]
    public async Task<IActionResult> DeleteCourse(long id)
    {
        await _adminService.DeleteCourseAsync(id);
        return NoContent();
    }

    [HttpPost("courses/{id}/approve")]
    public async Task<IActionResult> ApproveCourse(long id)
    {
        try
        {
            await _adminService.ApproveCourseAsync(id);
            return Ok(new { message = "Course approved successfully." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
