using System.Security.Cryptography;

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

using SkillHub.Application.Features.Identity.DTOs;
using SkillHub.Application.Features.Identity.Interfaces;
using SkillHub.Application.Features.Identity.Models;
using SkillHub.Domain.Entities.Identity;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Authentication;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;

    private readonly IJwtTokenGenerator _jwtTokenGenerator;

    private readonly JwtSettings _jwtSettings;

    public AuthService(
        ApplicationDbContext context,
        IJwtTokenGenerator jwtTokenGenerator,
        IOptions<JwtSettings> jwtSettings)
    {
        _context = context;

        _jwtTokenGenerator = jwtTokenGenerator;

        _jwtSettings = jwtSettings.Value;
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

        var studentRole = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Student");
        if (studentRole != null)
        {
            user.UserRoles.Add(new UserRole
            {
                RoleId = studentRole.Id,
                User = user
            });
        }

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();

        // Reload user with roles for JWT generation
        var registeredUser = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstAsync(u => u.Id == user.Id);

        var refreshToken = GenerateRefreshToken(registeredUser.Id);
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            UserId = registeredUser.Id,
            Email = registeredUser.Email,
            FullName = $"{registeredUser.FirstName} {registeredUser.LastName}",
            Token = _jwtTokenGenerator.GenerateToken(registeredUser),
            RefreshToken = refreshToken.Token,
            Roles = registeredUser.UserRoles
                .Select(ur => ur.Role.Name)
                .ToList()
        };

    }
    public async Task<AuthResponseDto> LoginAsync(
        LoginRequestDto request)
    {
        var user = await _context.Users
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
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

        var refreshToken = GenerateRefreshToken(user.Id);
        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync();

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = $"{user.FirstName} {user.LastName}",
            Token = _jwtTokenGenerator.GenerateToken(user),
            RefreshToken = refreshToken.Token,
            Roles = user.UserRoles
                .Select(ur => ur.Role.Name)
                .ToList()
        };
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(
        string token)
    {
        var existingToken = await _context.RefreshTokens
            .Include(rt => rt.User)
            .ThenInclude(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (existingToken == null)
        {
            throw new Exception("Invalid refresh token.");
        }

        if (!existingToken.IsActive)
        {
            throw new Exception("Refresh token has expired or been revoked.");
        }

        // Rotate: revoke the old token and create a new one
        existingToken.RevokedAt = DateTime.UtcNow;
        existingToken.ReplacedByToken = null; // will be set below

        var newRefreshToken =
            GenerateRefreshToken(existingToken.UserId);

        existingToken.ReplacedByToken = newRefreshToken.Token;

        await _context.RefreshTokens.AddAsync(newRefreshToken);
        await _context.SaveChangesAsync();

        var user = existingToken.User;

        return new AuthResponseDto
        {
            UserId = user.Id,
            Email = user.Email,
            FullName = $"{user.FirstName} {user.LastName}",
            Token = _jwtTokenGenerator.GenerateToken(user),
            RefreshToken = newRefreshToken.Token,
            Roles = user.UserRoles
                .Select(ur => ur.Role.Name)
                .ToList()
        };
    }

    public async Task LogoutAsync(string token)
    {
        var existingToken = await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (existingToken == null)
        {
            return; // If it's invalid or doesn't exist, they are effectively already logged out
        }

        if (existingToken.RevokedAt != null)
        {
            return; // Already revoked, nothing to do
        }

        existingToken.RevokedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    private RefreshToken GenerateRefreshToken(long userId)
    {
        var randomBytes = new byte[64];

        using var rng =
            RandomNumberGenerator.Create();

        rng.GetBytes(randomBytes);

        return new RefreshToken
        {
            UserId = userId,
            Token = Convert.ToBase64String(randomBytes),
            ExpiresAt = DateTime.UtcNow.AddDays(
                _jwtSettings.RefreshTokenDurationInDays > 0
                    ? _jwtSettings.RefreshTokenDurationInDays
                    : 7),
            CreatedAt = DateTime.UtcNow
        };
    }
}

//bcrypt is used to store the hashed password in the database and to verify
//the password during login. It provides a secure way to handle passwords by
//hashing them with a salt, making it difficult for attackers to retrieve the