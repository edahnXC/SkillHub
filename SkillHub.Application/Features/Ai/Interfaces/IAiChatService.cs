using System.Collections.Generic;
using System.Threading.Tasks;
using SkillHub.Application.Features.Ai.DTOs;

namespace SkillHub.Application.Features.Ai.Interfaces;

public interface IAiChatService
{
    Task<AiChatResponseDto> SendMessageAsync(AiChatRequestDto request, long? userId, string clientIp);
    List<string> GetQuickSuggestions(string? contextUrl);
}
