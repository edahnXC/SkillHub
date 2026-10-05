using System;
using System.Collections.Generic;
using SkillHub.Domain.Common;
using SkillHub.Domain.Entities.Identity;

namespace SkillHub.Domain.Entities.Support;

public class SupportTicket : BaseEntity
{
    public string TicketNumber { get; set; } = string.Empty;
    
    public long UserId { get; set; }
    public User User { get; set; } = null!;

    public string Subject { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Priority { get; set; } = "Medium";
    public string Status { get; set; } = "Open";

    public long? AssignedAdminId { get; set; }
    public User? AssignedAdmin { get; set; }

    public DateTime? ResolvedAt { get; set; }

    public ICollection<TicketMessage> Messages { get; set; } = new List<TicketMessage>();
}
