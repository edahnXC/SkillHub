using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SkillHub.Application.Features.Communication.Interfaces;
using SkillHub.Application.Features.Support.DTOs;
using SkillHub.Application.Features.Support.Interfaces;
using SkillHub.Domain.Entities.Support;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Services.Support;

public class SupportService : ISupportService
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<SupportService> _logger;

    public SupportService(
        ApplicationDbContext context,
        IEmailService emailService,
        ILogger<SupportService> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<SupportTicketDetailDto> CreateTicketAsync(long userId, CreateTicketRequestDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Subject))
            throw new ArgumentException("Subject cannot be empty.");
        if (string.IsNullOrWhiteSpace(dto.InitialMessage))
            throw new ArgumentException("Initial message cannot be empty.");

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            throw new KeyNotFoundException("User not found.");

        var count = await _context.SupportTickets.CountAsync() + 1;
        var ticketNumber = $"TICK-{DateTime.UtcNow:yyyy}-{count:D5}";

        var ticket = new SupportTicket
        {
            TicketNumber = ticketNumber,
            UserId = userId,
            Subject = dto.Subject.Trim(),
            Category = string.IsNullOrWhiteSpace(dto.Category) ? "GeneralInquiry" : dto.Category.Trim(),
            Priority = "Normal", // Initial triage status assigned automatically; Product Support assigns final urgency/priority
            Status = "Open",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var initialMessage = new TicketMessage
        {
            SenderUserId = userId,
            MessageText = dto.InitialMessage.Trim(),
            IsInternalNote = false,
            AttachmentUrl = dto.AttachmentUrl,
            CreatedAt = DateTime.UtcNow
        };

        ticket.Messages.Add(initialMessage);
        _context.SupportTickets.Add(ticket);
        await _context.SaveChangesAsync();

        // Dispatch free transactional notification email
        try
        {
            var fullName = $"{user.FirstName} {user.LastName}".Trim();
            await _emailService.SendTicketCreatedEmailAsync(user.Email, fullName, ticket.TicketNumber, ticket.Subject, ticket.Category);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send ticket creation email for ticket {TicketNumber}", ticket.TicketNumber);
        }

        return await GetTicketDetailInternalAsync(ticket.Id, userId, isStaff: false);
    }

    public async Task<List<SupportTicketDto>> GetUserTicketsAsync(long userId)
    {
        return await _context.SupportTickets
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
            .Select(t => new SupportTicketDto
            {
                Id = t.Id,
                TicketNumber = t.TicketNumber,
                UserId = t.UserId,
                UserName = t.User.FirstName + " " + t.User.LastName,
                UserEmail = t.User.Email,
                Subject = t.Subject,
                Category = t.Category,
                Priority = t.Priority,
                Status = t.Status,
                AssignedAdminId = t.AssignedAdminId,
                AssignedAdminName = t.AssignedAdmin != null ? t.AssignedAdmin.FirstName + " " + t.AssignedAdmin.LastName : null,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                ResolvedAt = t.ResolvedAt,
                MessagesCount = t.Messages.Count(m => !m.IsInternalNote)
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<SupportTicketDetailDto> GetTicketDetailAsync(long ticketId, long currentUserId, bool isStaff)
    {
        return await GetTicketDetailInternalAsync(ticketId, currentUserId, isStaff);
    }

    public async Task<TicketMessageDto> AddMessageAsync(long ticketId, long userId, AddTicketMessageRequestDto dto, bool isStaff)
    {
        if (string.IsNullOrWhiteSpace(dto.MessageText))
            throw new ArgumentException("Message cannot be empty.");

        var ticket = await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.AssignedAdmin)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            throw new KeyNotFoundException($"Support ticket {ticketId} not found.");

        if (!isStaff && ticket.UserId != userId)
            throw new UnauthorizedAccessException("You can only respond to your own support tickets.");

        var isInternal = isStaff && dto.IsInternalNote;

        var message = new TicketMessage
        {
            TicketId = ticketId,
            SenderUserId = userId,
            MessageText = dto.MessageText.Trim(),
            IsInternalNote = isInternal,
            AttachmentUrl = dto.AttachmentUrl,
            CreatedAt = DateTime.UtcNow
        };

        // Automatic state machine progression
        if (!isInternal)
        {
            if (isStaff && ticket.Status is "Open" or "InProgress")
            {
                ticket.Status = "WaitingOnUser";
            }
            else if (!isStaff && ticket.Status is "WaitingOnUser" or "Resolved")
            {
                ticket.Status = "InProgress";
            }
        }

        ticket.UpdatedAt = DateTime.UtcNow;
        _context.TicketMessages.Add(message);
        await _context.SaveChangesAsync();

        var sender = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var senderName = sender != null ? $"{sender.FirstName} {sender.LastName}".Trim() : "Staff";
        var senderRole = sender?.UserRoles.FirstOrDefault()?.Role?.Name ?? "Student";

        // Dispatch free transactional notification email
        if (!isInternal)
        {
            try
            {
                if (isStaff && !string.IsNullOrWhiteSpace(ticket.User?.Email))
                {
                    var studentName = $"{ticket.User.FirstName} {ticket.User.LastName}".Trim();
                    await _emailService.SendTicketReplyEmailAsync(
                        ticket.User.Email,
                        studentName,
                        ticket.TicketNumber,
                        message.MessageText,
                        senderName,
                        isStaffReply: true);
                }
                else if (!isStaff && ticket.AssignedAdmin != null && !string.IsNullOrWhiteSpace(ticket.AssignedAdmin.Email))
                {
                    var adminName = $"{ticket.AssignedAdmin.FirstName} {ticket.AssignedAdmin.LastName}".Trim();
                    await _emailService.SendTicketReplyEmailAsync(
                        ticket.AssignedAdmin.Email,
                        adminName,
                        ticket.TicketNumber,
                        message.MessageText,
                        senderName,
                        isStaffReply: false);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send ticket response email for ticket {TicketNumber}", ticket.TicketNumber);
            }
        }

        return new TicketMessageDto
        {
            Id = message.Id,
            TicketId = message.TicketId,
            SenderUserId = message.SenderUserId,
            SenderName = senderName,
            SenderRole = senderRole,
            MessageText = message.MessageText,
            IsInternalNote = message.IsInternalNote,
            AttachmentUrl = message.AttachmentUrl,
            CreatedAt = message.CreatedAt
        };
    }

    public async Task<List<SupportTicketDto>> GetAllTicketsAsync(
        string? status,
        string? priority,
        string? category,
        long? assignedToId,
        string? search)
    {
        var query = _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.AssignedAdmin)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(status) && !string.Equals(status, "ALL", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(t => t.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(priority) && !string.Equals(priority, "ALL", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(t => t.Priority == priority);
        }

        if (!string.IsNullOrWhiteSpace(category) && !string.Equals(category, "ALL", StringComparison.OrdinalIgnoreCase))
        {
            query = query.Where(t => t.Category == category);
        }

        if (assignedToId.HasValue)
        {
            query = query.Where(t => t.AssignedAdminId == assignedToId.Value);
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(t =>
                t.TicketNumber.ToLower().Contains(s) ||
                t.Subject.ToLower().Contains(s) ||
                t.User.Email.ToLower().Contains(s) ||
                (t.User.FirstName + " " + t.User.LastName).ToLower().Contains(s));
        }

        return await query
            .OrderByDescending(t => t.UpdatedAt ?? t.CreatedAt)
            .Select(t => new SupportTicketDto
            {
                Id = t.Id,
                TicketNumber = t.TicketNumber,
                UserId = t.UserId,
                UserName = t.User.FirstName + " " + t.User.LastName,
                UserEmail = t.User.Email,
                Subject = t.Subject,
                Category = t.Category,
                Priority = t.Priority,
                Status = t.Status,
                AssignedAdminId = t.AssignedAdminId,
                AssignedAdminName = t.AssignedAdmin != null ? t.AssignedAdmin.FirstName + " " + t.AssignedAdmin.LastName : null,
                CreatedAt = t.CreatedAt,
                UpdatedAt = t.UpdatedAt,
                ResolvedAt = t.ResolvedAt,
                MessagesCount = t.Messages.Count
            })
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<SupportTicketDto> UpdateStatusAsync(long ticketId, string status, long adminId)
    {
        var ticket = await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.AssignedAdmin)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            throw new KeyNotFoundException($"Support ticket {ticketId} not found.");

        ticket.Status = status.Trim();
        ticket.UpdatedAt = DateTime.UtcNow;

        if (string.Equals(ticket.Status, "Resolved", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(ticket.Status, "Closed", StringComparison.OrdinalIgnoreCase))
        {
            ticket.ResolvedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        try
        {
            if (!string.IsNullOrWhiteSpace(ticket.User?.Email))
            {
                var studentName = $"{ticket.User.FirstName} {ticket.User.LastName}".Trim();
                await _emailService.SendTicketStatusChangedEmailAsync(ticket.User.Email, studentName, ticket.TicketNumber, ticket.Status);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to send status update email for ticket {TicketNumber}", ticket.TicketNumber);
        }

        return new SupportTicketDto
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            UserId = ticket.UserId,
            UserName = ticket.User != null ? $"{ticket.User.FirstName} {ticket.User.LastName}" : string.Empty,
            UserEmail = ticket.User?.Email ?? string.Empty,
            Subject = ticket.Subject,
            Category = ticket.Category,
            Priority = ticket.Priority,
            Status = ticket.Status,
            AssignedAdminId = ticket.AssignedAdminId,
            AssignedAdminName = ticket.AssignedAdmin != null ? $"{ticket.AssignedAdmin.FirstName} {ticket.AssignedAdmin.LastName}" : null,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            ResolvedAt = ticket.ResolvedAt,
            MessagesCount = await _context.TicketMessages.CountAsync(m => m.TicketId == ticket.Id)
        };
    }

    public async Task<SupportTicketDto> UpdatePriorityAsync(long ticketId, string priority, long adminId)
    {
        var ticket = await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.AssignedAdmin)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            throw new KeyNotFoundException($"Support ticket {ticketId} not found.");

        ticket.Priority = priority.Trim();
        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new SupportTicketDto
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            UserId = ticket.UserId,
            UserName = ticket.User != null ? $"{ticket.User.FirstName} {ticket.User.LastName}" : string.Empty,
            UserEmail = ticket.User?.Email ?? string.Empty,
            Subject = ticket.Subject,
            Category = ticket.Category,
            Priority = ticket.Priority,
            Status = ticket.Status,
            AssignedAdminId = ticket.AssignedAdminId,
            AssignedAdminName = ticket.AssignedAdmin != null ? $"{ticket.AssignedAdmin.FirstName} {ticket.AssignedAdmin.LastName}" : null,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            ResolvedAt = ticket.ResolvedAt,
            MessagesCount = await _context.TicketMessages.CountAsync(m => m.TicketId == ticket.Id)
        };
    }

    public async Task<SupportTicketDto> AssignTicketAsync(long ticketId, long? assignedAdminId, long adminId)
    {
        var ticket = await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.AssignedAdmin)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            throw new KeyNotFoundException($"Support ticket {ticketId} not found.");

        if (assignedAdminId.HasValue)
        {
            var isSuperAdmin = await _context.UserRoles
                .AnyAsync(ur => ur.UserId == assignedAdminId.Value && ur.Role.Name == "SuperAdmin");
            if (isSuperAdmin)
            {
                throw new InvalidOperationException("SuperAdmins have read-only oversight and cannot be assigned tickets.");
            }
        }

        ticket.AssignedAdminId = assignedAdminId;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var admin = assignedAdminId.HasValue 
            ? await _context.Users.FirstOrDefaultAsync(u => u.Id == assignedAdminId.Value) 
            : null;

        return new SupportTicketDto
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            UserId = ticket.UserId,
            UserName = ticket.User != null ? $"{ticket.User.FirstName} {ticket.User.LastName}" : string.Empty,
            UserEmail = ticket.User?.Email ?? string.Empty,
            Subject = ticket.Subject,
            Category = ticket.Category,
            Priority = ticket.Priority,
            Status = ticket.Status,
            AssignedAdminId = ticket.AssignedAdminId,
            AssignedAdminName = admin != null ? $"{admin.FirstName} {admin.LastName}" : null,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            ResolvedAt = ticket.ResolvedAt,
            MessagesCount = await _context.TicketMessages.CountAsync(m => m.TicketId == ticket.Id)
        };
    }

    private async Task<SupportTicketDetailDto> GetTicketDetailInternalAsync(long ticketId, long currentUserId, bool isStaff)
    {
        var ticket = await _context.SupportTickets
            .Include(t => t.User)
            .Include(t => t.AssignedAdmin)
            .Include(t => t.Messages)
                .ThenInclude(m => m.SenderUser)
                    .ThenInclude(u => u.UserRoles)
                        .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            throw new KeyNotFoundException($"Ticket {ticketId} not found.");

        if (!isStaff && ticket.UserId != currentUserId)
            throw new UnauthorizedAccessException("Access denied. You do not have permission to view this ticket.");

        var rawMessages = isStaff
            ? ticket.Messages
            : ticket.Messages.Where(m => !m.IsInternalNote);

        var messageDtos = rawMessages
            .OrderBy(m => m.CreatedAt)
            .Select(m => new TicketMessageDto
            {
                Id = m.Id,
                TicketId = m.TicketId,
                SenderUserId = m.SenderUserId,
                SenderName = m.SenderUser != null ? $"{m.SenderUser.FirstName} {m.SenderUser.LastName}".Trim() : "User",
                SenderRole = m.SenderUser?.UserRoles.FirstOrDefault()?.Role?.Name ?? "Student",
                MessageText = m.MessageText,
                IsInternalNote = m.IsInternalNote,
                AttachmentUrl = m.AttachmentUrl,
                CreatedAt = m.CreatedAt
            })
            .ToList();

        return new SupportTicketDetailDto
        {
            Id = ticket.Id,
            TicketNumber = ticket.TicketNumber,
            UserId = ticket.UserId,
            UserName = ticket.User != null ? $"{ticket.User.FirstName} {ticket.User.LastName}" : string.Empty,
            UserEmail = ticket.User?.Email ?? string.Empty,
            Subject = ticket.Subject,
            Category = ticket.Category,
            Priority = ticket.Priority,
            Status = ticket.Status,
            AssignedAdminId = ticket.AssignedAdminId,
            AssignedAdminName = ticket.AssignedAdmin != null ? $"{ticket.AssignedAdmin.FirstName} {ticket.AssignedAdmin.LastName}" : null,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            ResolvedAt = ticket.ResolvedAt,
            MessagesCount = messageDtos.Count,
            Messages = messageDtos
        };
    }
}
