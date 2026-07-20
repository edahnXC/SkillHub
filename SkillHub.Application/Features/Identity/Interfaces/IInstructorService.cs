using SkillHub.Application.Features.Identity.DTOs;

namespace SkillHub.Application.Features.Identity.Interfaces;

public interface IInstructorService
{
    Task<List<InstructorDto>> SearchInstructorsAsync(string? query);
    Task<InstructorDto?> GetInstructorByIdAsync(long id);
}
