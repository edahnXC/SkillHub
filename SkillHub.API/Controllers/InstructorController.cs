using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Features.Identity.DTOs;
using SkillHub.Application.Features.Identity.Interfaces;

namespace SkillHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstructorController : ControllerBase
{
    private readonly IInstructorService _instructorService;

    public InstructorController(IInstructorService instructorService)
    {
        _instructorService = instructorService;
    }

    /// <summary>
    /// Search/list instructors. Supports search by ID, name, or email.
    /// </summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<List<InstructorDto>>> Search(
        [FromQuery] string? query)
    {
        var instructors = await _instructorService.SearchInstructorsAsync(query);
        return Ok(instructors);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<ActionResult<InstructorDto>> GetById(long id)
    {
        var instructor = await _instructorService.GetInstructorByIdAsync(id);
        if (instructor == null)
        {
            return NotFound();
        }
        return Ok(instructor);
    }
}
