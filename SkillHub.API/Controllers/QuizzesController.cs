using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Features.Assessments.DTOs;
using SkillHub.Application.Features.Assessments.Interfaces;

namespace SkillHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizzesController : ControllerBase
    {
        private readonly IQuizService _quizService;

        public QuizzesController(IQuizService quizService)
        {
            _quizService = quizService;
        }

        private long? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            return null;
        }

        [HttpGet("course/{courseId}")]
        [Authorize]
        public async Task<IActionResult> GetCourseQuizzes(long courseId)
        {
            var userId = GetCurrentUserId();
            var quizzes = await _quizService.GetQuizzesByCourseIdAsync(courseId, userId);
            return Ok(quizzes);
        }

        [HttpGet("{quizId}")]
        [Authorize]
        public async Task<IActionResult> GetQuizById(long quizId)
        {
            var userId = GetCurrentUserId();
            var quiz = await _quizService.GetQuizByIdAsync(quizId, userId);
            if (quiz == null) return NotFound();
            return Ok(quiz);
        }

        [HttpPost("submit")]
        [Authorize]
        public async Task<IActionResult> SubmitQuizAttempt([FromBody] SubmitQuizAttemptRequestDto dto)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            try
            {
                var result = await _quizService.SubmitQuizAttemptAsync(userId.Value, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPost("create")]
        [Authorize(Roles = "Instructor,SuperAdmin,SubAdmin")]
        public async Task<IActionResult> CreateQuiz([FromBody] CreateQuizRequestDto dto)
        {
            var instructorId = GetCurrentUserId();
            if (instructorId == null) return Unauthorized();

            try
            {
                var result = await _quizService.CreateQuizAsync(instructorId.Value, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
