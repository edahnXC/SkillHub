using SkillHub.Application.Features.Profile.DTOs;

namespace SkillHub.Application.Features.Profile.Interfaces;

public interface IProfileService
{
    Task<ProfileResponseDto?> GetProfileAsync(long userId);
    Task<bool> UpdateProfileAsync(long userId, UpdateProfileRequestDto dto);
    Task<(bool Success, string Message)> ChangePasswordAsync(long userId, ChangePasswordRequestDto dto);
}
