using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Features.Support.DTOs;
using SkillHub.Application.Features.Support.Interfaces;

namespace SkillHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SupportController : ControllerBase
{
    private readonly ISupportService _supportService;

    public SupportController(ISupportService supportService)
    {
        _supportService = supportService;
    }

    private long GetUserId()
    {
        var idStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return long.TryParse(idStr, out var id) ? id : 0;
    }

    private bool IsStaff()
    {
        return User.IsInRole("SuperAdmin") || User.IsInRole("SubAdmin");
    }

    [HttpPost("tickets")]
    public async Task<IActionResult> CreateTicket([FromBody] CreateTicketRequestDto dto)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var ticket = await _supportService.CreateTicketAsync(userId, dto);
        return CreatedAtAction(nameof(GetTicketDetail), new { id = ticket.Id }, ticket);
    }

    [HttpGet("my-tickets")]
    public async Task<IActionResult> GetMyTickets()
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var tickets = await _supportService.GetUserTicketsAsync(userId);
        return Ok(tickets);
    }

    [HttpGet("tickets/{id}")]
    public async Task<IActionResult> GetTicketDetail(long id)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var ticket = await _supportService.GetTicketDetailAsync(id, userId, IsStaff());
        return Ok(ticket);
    }

    [HttpPost("tickets/{id}/messages")]
    public async Task<IActionResult> AddMessage(long id, [FromBody] AddTicketMessageRequestDto dto)
    {
        var userId = GetUserId();
        if (userId == 0) return Unauthorized();

        var message = await _supportService.AddMessageAsync(id, userId, dto, IsStaff());
        return Ok(message);
    }

    [HttpGet("admin/queue")]
    [Authorize(Roles = "SuperAdmin,SubAdmin")]
    public async Task<IActionResult> GetAdminQueue(
        [FromQuery] string? status,
        [FromQuery] string? priority,
        [FromQuery] string? category,
        [FromQuery] long? assignedToId,
        [FromQuery] string? search)
    {
        var tickets = await _supportService.GetAllTicketsAsync(status, priority, category, assignedToId, search);
        return Ok(tickets);
    }

    [HttpPut("admin/tickets/{id}/status")]
    [Authorize(Roles = "SuperAdmin,SubAdmin")]
    public async Task<IActionResult> UpdateTicketStatus(long id, [FromBody] UpdateTicketStatusRequestDto dto)
    {
        var adminId = GetUserId();
        var ticket = await _supportService.UpdateStatusAsync(id, dto.Status, adminId);
        return Ok(ticket);
    }

    [HttpPut("admin/tickets/{id}/assign")]
    [Authorize(Roles = "SuperAdmin,SubAdmin")]
    public async Task<IActionResult> AssignTicket(long id, [FromBody] AssignTicketRequestDto dto)
    {
        var adminId = GetUserId();
        var ticket = await _supportService.AssignTicketAsync(id, dto.AssignedAdminId, adminId);
        return Ok(ticket);
    }
}
