using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Features.Admin.Interfaces;

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
        await _adminService.DeleteUserAsync(id);
        return NoContent();
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
}
