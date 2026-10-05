using System;
using SkillHub.Domain.Common;
using SkillHub.Domain.Entities.Identity;

namespace SkillHub.Domain.Entities.Support;

public class TicketMessage : BaseEntity
{
    public long TicketId { get; set; }
    public SupportTicket Ticket { get; set; } = null!;

    public long SenderUserId { get; set; }
    public User SenderUser { get; set; } = null!;

    public string MessageText { get; set; } = string.Empty;

    // Allows staff to leave private notes hidden from students
    public bool IsInternalNote { get; set; } = false;

    public string? AttachmentUrl { get; set; }
}
