using System.Collections.Generic;
using System.Threading.Tasks;
using SkillHub.Application.Features.Support.DTOs;

namespace SkillHub.Application.Features.Support.Interfaces;

public interface ISupportService
{
    Task<SupportTicketDetailDto> CreateTicketAsync(long userId, CreateTicketRequestDto dto);
    Task<List<SupportTicketDto>> GetUserTicketsAsync(long userId);
    Task<SupportTicketDetailDto> GetTicketDetailAsync(long ticketId, long currentUserId, bool isStaff);
    Task<TicketMessageDto> AddMessageAsync(long ticketId, long userId, AddTicketMessageRequestDto dto, bool isStaff);
    Task<List<SupportTicketDto>> GetAllTicketsAsync(string? status, string? priority, string? category, long? assignedToId, string? search);
    Task<SupportTicketDto> UpdateStatusAsync(long ticketId, string status, long adminId);
    Task<SupportTicketDto> UpdatePriorityAsync(long ticketId, string priority, long adminId);
    Task<SupportTicketDto> AssignTicketAsync(long ticketId, long? assignedAdminId, long adminId);
}
