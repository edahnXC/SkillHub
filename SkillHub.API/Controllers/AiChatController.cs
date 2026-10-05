using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Features.Ai.DTOs;
using SkillHub.Application.Features.Ai.Interfaces;

namespace SkillHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AiChatController : ControllerBase
{
    private readonly IAiChatService _aiChatService;

    public AiChatController(IAiChatService aiChatService)
    {
        _aiChatService = aiChatService;
    }

    private long? GetUserId()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(idStr, out var id) ? id : null;
    }

    [HttpPost("message")]
    [AllowAnonymous]
    public async Task<IActionResult> SendMessage([FromBody] AiChatRequestDto request)
    {
        var userId = GetUserId();
        var clientIp = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        var response = await _aiChatService.SendMessageAsync(request, userId, clientIp);
        return Ok(response);
    }

    [HttpGet("suggestions")]
    [AllowAnonymous]
    public IActionResult GetSuggestions([FromQuery] string? route)
    {
        var suggestions = _aiChatService.GetQuickSuggestions(route);
        return Ok(suggestions);
    }
}
