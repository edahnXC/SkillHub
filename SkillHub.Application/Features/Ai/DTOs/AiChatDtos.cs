using System;
using System.Collections.Generic;

namespace SkillHub.Application.Features.Ai.DTOs;

public class AiChatMessageDto
{
    public string Role { get; set; } = "user"; // "user" or "model"
    public string Text { get; set; } = string.Empty;
    public DateTime? Timestamp { get; set; }
}

public class AiChatRequestDto
{
    public string Message { get; set; } = string.Empty;
    public string? ContextUrl { get; set; }
    public List<AiChatMessageDto> History { get; set; } = new();
    public string? GuestSessionId { get; set; }
}

public class AiChatResponseDto
{
    public string Reply { get; set; } = string.Empty;
    public List<string> Suggestions { get; set; } = new();
    public int? RemainingGuestQuota { get; set; }
    public bool IsGuestLimitReached { get; set; } = false;
    public string ModelUsed { get; set; } = "gemini-1.5-flash";
}
