using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Features.Courses.Interfaces;

namespace SkillHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Student")]
public class EnrollmentController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpPost("enroll/{courseId}")]
    public async Task<IActionResult> Enroll(long courseId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdClaim, out var studentId))
        {
            return Unauthorized("User ID not found or invalid.");
        }

        try
        {
            var success = await _enrollmentService.EnrollStudentAsync(studentId, courseId);
            if (!success)
            {
                return BadRequest("Already enrolled in this course.");
            }
            return Ok(new { message = "Enrolled successfully." });
        }
        catch (ArgumentException ex)
        {
            return NotFound(ex.Message);
        }
    }

    [HttpGet("my-learning")]
    public async Task<IActionResult> GetMyLearning()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdClaim, out var studentId))
        {
            return Unauthorized("User ID not found or invalid.");
        }

        var courses = await _enrollmentService.GetStudentEnrollmentsAsync(studentId);
        return Ok(courses);
    }

    [HttpGet("check/{courseId}")]
    public async Task<IActionResult> CheckEnrollment(long courseId)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!long.TryParse(userIdClaim, out var studentId))
        {
            return Unauthorized("User ID not found or invalid.");
        }

        var isEnrolled = await _enrollmentService.IsEnrolledAsync(studentId, courseId);
        return Ok(isEnrolled);
    }
}
