using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
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
        return User.IsInRole("SuperAdmin") || User.IsInRole("SubAdmin") || User.IsInRole("ProductSupport");
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

        // SuperAdmins have read-only oversight across platform support tickets
        if (User.IsInRole("SuperAdmin"))
        {
            var ticketCheck = await _supportService.GetTicketDetailAsync(id, userId, true);
            if (ticketCheck.UserId != userId)
            {
                return StatusCode(StatusCodes.Status403Forbidden, new { message = "SuperAdmins have read-only oversight on support tickets and cannot post replies." });
            }
        }

        var message = await _supportService.AddMessageAsync(id, userId, dto, IsStaff());
        return Ok(message);
    }

    [HttpGet("admin/queue")]
    [Authorize(Roles = "SuperAdmin,SubAdmin,ProductSupport")]
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
    [Authorize(Roles = "SubAdmin,ProductSupport,SuperAdmin")]
    public async Task<IActionResult> UpdateTicketStatus(long id, [FromBody] UpdateTicketStatusRequestDto dto)
    {
        if (User.IsInRole("SuperAdmin"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "SuperAdmins have read-only oversight and cannot modify ticket status." });
        }

        var adminId = GetUserId();
        var ticket = await _supportService.UpdateStatusAsync(id, dto.Status, adminId);
        return Ok(ticket);
    }

    [HttpGet("admin/assignable-admins")]
    [Authorize(Roles = "SuperAdmin,SubAdmin,ProductSupport")]
    public async Task<IActionResult> GetAssignableAdmins([FromServices] SkillHub.Application.Features.Admin.Interfaces.IAdminService adminService)
    {
        var users = await adminService.GetAllUsersAsync();
        var admins = users.Where(u => u.Roles.Contains("SuperAdmin") || u.Roles.Contains("SubAdmin")).ToList();
        return Ok(admins);
    }

    [HttpPut("admin/tickets/{id}/assign")]
    [Authorize(Roles = "ProductSupport,SuperAdmin,SubAdmin")]
    public async Task<IActionResult> AssignTicket(long id, [FromBody] AssignTicketRequestDto dto)
    {
        if (!User.IsInRole("ProductSupport"))
        {
            return StatusCode(StatusCodes.Status403Forbidden, new { message = "Ticket assignment is exclusively managed by Product Support." });
        }

        var adminId = GetUserId();
        var ticket = await _supportService.AssignTicketAsync(id, dto.AssignedAdminId, adminId);
        return Ok(ticket);
    }
}
