using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using SkillHub.Application.Features.Courses.DTOs;
using SkillHub.Application.Features.Courses.Interfaces;

namespace SkillHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CourseController : ControllerBase
{
    private readonly ICourseService
        _courseService;

    public CourseController(
        ICourseService courseService)
    {
        _courseService = courseService;
    }

    [HttpPost]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult>
        Create(
            CreateCourseRequestDto request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdClaim, out var instructorId))
        {
            return Unauthorized("User ID not found or invalid.");
        }

        var result =
            await _courseService
                .CreateAsync(request, instructorId);

        return Ok(result);
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult>
        GetAll()
    {
        var result =
            await _courseService
                .GetAllAsync();

        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult>
        GetById(
            long id)
    {
        var result =
            await _courseService
                .GetByIdAsync(id);

        if (result == null)
        {
            return NotFound();
        }

        return Ok(result);
    }

    [HttpGet("instructor/{instructorId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetByInstructor(long instructorId)
    {
        var result = await _courseService.GetCoursesByInstructorAsync(instructorId);
        return Ok(result);
    }

    [HttpPut]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult>
        Update(
            UpdateCourseRequestDto request)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdClaim, out var instructorId))
        {
            return Unauthorized("User ID not found or invalid.");
        }

        try
        {
            await _courseService.UpdateAsync(request, instructorId);
            return Ok();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Instructor")]
    public async Task<IActionResult>
        Delete(
            long id)
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdClaim, out var instructorId))
        {
            return Unauthorized("User ID not found or invalid.");
        }

        try
        {
            await _courseService.DeleteAsync(id, instructorId);
            return Ok();
        }
        catch (UnauthorizedAccessException ex)
        {
            return Forbid(ex.Message);
        }
    }
}
//list of instructors in grid
//search instructors by name, email, or expertise