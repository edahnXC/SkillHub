using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Features.Profile.DTOs;
using SkillHub.Application.Features.Profile.Interfaces;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Services.Identity;

public class ProfileService : IProfileService
{
    private readonly ApplicationDbContext _context;

    public ProfileService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ProfileResponseDto?> GetProfileAsync(long userId)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
                .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null) return null;

        var roles = user.UserRoles
            .Select(ur => ur.Role.Name)
            .ToList();

        return new ProfileResponseDto(
            user.Id,
            user.FirstName,
            user.LastName,
            user.Email,
            user.PhoneNumber,
            user.ProfileImageUrl,
            roles,
            user.CreatedAt
        );
    }

    public async Task<bool> UpdateProfileAsync(long userId, UpdateProfileRequestDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return false;

        user.FirstName = dto.FirstName.Trim();
        user.LastName = dto.LastName.Trim();
        user.PhoneNumber = dto.PhoneNumber?.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<(bool Success, string Message)> ChangePasswordAsync(long userId, ChangePasswordRequestDto dto)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return (false, "User not found");

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            return (false, "Current password is incorrect");

        if (dto.NewPassword.Length < 6)
            return (false, "New password must be at least 6 characters long");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return (true, "Password updated successfully");
    }
}
