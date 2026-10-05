using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Features.Courses.DTOs;
using SkillHub.Application.Features.Courses.Interfaces;

namespace SkillHub.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurriculumController : ControllerBase
    {
        private readonly ICurriculumService _curriculumService;

        public CurriculumController(ICurriculumService curriculumService)
        {
            _curriculumService = curriculumService;
        }

        private long? GetInstructorId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (long.TryParse(userIdClaim, out var instructorId))
            {
                return instructorId;
            }
            return null;
        }

        [HttpGet("course/{courseId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetCurriculumByCourse(long courseId)
        {
            var curriculum = await _curriculumService.GetCurriculumByCourseIdAsync(courseId);
            return Ok(curriculum);
        }

        [HttpPost("sections")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> CreateSection([FromBody] CreateSectionRequestDto dto)
        {
            var instructorId = GetInstructorId();
            if (instructorId == null) return Unauthorized("Invalid user token.");

            try
            {
                var result = await _curriculumService.CreateSectionAsync(dto, instructorId.Value);
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
        }

        [HttpPut("sections")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> UpdateSection([FromBody] UpdateSectionRequestDto dto)
        {
            var instructorId = GetInstructorId();
            if (instructorId == null) return Unauthorized("Invalid user token.");

            try
            {
                await _curriculumService.UpdateSectionAsync(dto, instructorId.Value);
                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("sections/{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> DeleteSection(long id)
        {
            var instructorId = GetInstructorId();
            if (instructorId == null) return Unauthorized("Invalid user token.");

            try
            {
                await _curriculumService.DeleteSectionAsync(id, instructorId.Value);
                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("lessons")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> CreateLesson([FromBody] CreateLessonRequestDto dto)
        {
            var instructorId = GetInstructorId();
            if (instructorId == null) return Unauthorized("Invalid user token.");

            try
            {
                var result = await _curriculumService.CreateLessonAsync(dto, instructorId.Value);
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
        }

        [HttpPut("lessons")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> UpdateLesson([FromBody] UpdateLessonRequestDto dto)
        {
            var instructorId = GetInstructorId();
            if (instructorId == null) return Unauthorized("Invalid user token.");

            try
            {
                await _curriculumService.UpdateLessonAsync(dto, instructorId.Value);
                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpDelete("lessons/{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> DeleteLesson(long id)
        {
            var instructorId = GetInstructorId();
            if (instructorId == null) return Unauthorized("Invalid user token.");

            try
            {
                await _curriculumService.DeleteLessonAsync(id, instructorId.Value);
                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpPost("resources")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> AddResource([FromBody] CreateLessonResourceRequestDto dto)
        {
            var instructorId = GetInstructorId();
            if (instructorId == null) return Unauthorized("Invalid user token.");

            try
            {
                var result = await _curriculumService.AddLessonResourceAsync(dto, instructorId.Value);
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
        }

        [HttpDelete("resources/{id}")]
        [Authorize(Roles = "Instructor")]
        public async Task<IActionResult> DeleteResource(long id)
        {
            var instructorId = GetInstructorId();
            if (instructorId == null) return Unauthorized("Invalid user token.");

            try
            {
                await _curriculumService.DeleteLessonResourceAsync(id, instructorId.Value);
                return Ok();
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid(ex.Message);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }
}
