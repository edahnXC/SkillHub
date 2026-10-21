using System;
using System.Collections.Generic;

namespace SkillHub.Application.Features.Support.DTOs;

public class CreateTicketRequestDto
{
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string InitialMessage { get; set; } = string.Empty;
    public string? AttachmentUrl { get; set; }
}

public class AddTicketMessageRequestDto
{
    public string MessageText { get; set; } = string.Empty;
    public bool IsInternalNote { get; set; } = false;
    public string? AttachmentUrl { get; set; }
}

public class UpdateTicketPriorityRequestDto
{
    public string Priority { get; set; } = string.Empty;
}

public class UpdateTicketStatusRequestDto
{
    public string Status { get; set; } = string.Empty;
}

public class AssignTicketRequestDto
{
    public long? AssignedAdminId { get; set; }
}

public class TicketMessageDto
{
    public long Id { get; set; }
    public long TicketId { get; set; }
    public long SenderUserId { get; set; }
    public string SenderName { get; set; } = string.Empty;
    public string SenderRole { get; set; } = string.Empty;
    public string MessageText { get; set; } = string.Empty;
    public bool IsInternalNote { get; set; }
    public string? AttachmentUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class SupportTicketDto
{
    public long Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public long UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserEmail { get; set; } = string.Empty;
    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public long? AssignedAdminId { get; set; }
    public string? AssignedAdminName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public int MessagesCount { get; set; }
}

public class SupportTicketDetailDto : SupportTicketDto
{
    public List<TicketMessageDto> Messages { get; set; } = new();
}
