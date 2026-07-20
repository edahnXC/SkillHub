using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Features.Files.Interfaces;

namespace SkillHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IFileService _fileService;

    public UploadController(IFileService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost("image")]
    [Authorize(Roles = "Admin,Instructor")]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        try
        {
            var url = await _fileService.UploadImageAsync(file, "thumbnails");
            return Ok(new { url });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(500, new { message = "An error occurred while uploading the file." });
        }
    }
}
