using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Features.Identity.DTOs;
using SkillHub.Application.Features.Identity.Interfaces;
using SkillHub.Domain.Entities.Identity;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Authentication;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;

    public AuthService(
        ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<AuthResponseDto> RegisterAsync(
        RegisterRequestDto request)
    {
        var existingUser = await _context.Users.
            FirstOrDefaultAsync(x => x.Email == request.Email);
        if (existingUser != null)
        {
            throw new Exception("Email already exists");
        }

        var user = new User
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,

            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            IsActive = true,
            IsEmailVerified = false
        };

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return new AuthResponseDto
        {
            Email = user.Email,
            FullName = $"{user.FirstName} {user.LastName}",
            Token = string.Empty
        };

    }
    public async Task<AuthResponseDto> LoginAsync(
        LoginRequestDto request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(x => x.Email == request.Email);

        if (user == null)
        {
            throw new Exception("Invalid credentials.");
        }

        bool isValid =
            BCrypt.Net.BCrypt.Verify(
                request.Password,
                user.PasswordHash);

        if (!isValid)
        {
            throw new Exception("Invalid credentials.");
        }

        return new AuthResponseDto
        {
            Email = user.Email,
            FullName = $"{user.FirstName} {user.LastName}",
            Token = string.Empty
        };
    }
}

//bcrypt is used to store the hashed password in the database and to verify
//the password during login. It provides a secure way to handle passwords by
//hashing them with a salt, making it difficult for attackers to retrieve the