using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SkillHub.Application.Features.Identity.DTOs;
using SkillHub.Application.Features.Identity.Interfaces;

namespace SkillHub.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    public AuthController(
        IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]  //when agular sends a request to api/auth/register, this method will be called
    public async Task<IActionResult> Register(
        RegisterRequestDto request)
    {
        var result = await _authService.RegisterAsync(request);
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(
        LoginRequestDto request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result);
        }
        catch (Exception ex) when (ex.Message == "Invalid credentials." || ex.Message == "Account is deactivated.")
        {
            return Unauthorized(new { message = ex.Message });
        }
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(
        RefreshTokenRequestDto request)
    {
        var result = await _authService
            .RefreshTokenAsync(request.Token);
        return Ok(result);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(
        RefreshTokenRequestDto request)
    {
        await _authService.LogoutAsync(request.Token);
        return Ok(new { message = "Logged out successfully." });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
        [FromBody] ForgotPasswordDto request, 
        [FromServices] SkillHub.Infrastructure.Persistence.Context.ApplicationDbContext dbContext,
        [FromServices] SkillHub.Application.Features.Communication.Interfaces.IEmailService emailService)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
        {
            return BadRequest(new { message = "Email is required." });
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        var user = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
            dbContext.Users, u => u.Email.ToLower() == normalizedEmail);

        if (user == null)
        {
            return Ok(new { 
                message = "If this email is registered, you will receive a reset code shortly."
            });
        }

        var resetCode = System.Security.Cryptography.RandomNumberGenerator.GetInt32(100000, 999999).ToString();
        ResetCodeStore[normalizedEmail] = (resetCode, DateTime.UtcNow.AddMinutes(15));

        // Dispatch transactional email (sent via SMTP if configured, or logged in console)
        var emailSubject = "SkillHub - Password Reset Verification Code";
        var emailBody = $@"
            <div style='font-family: Arial, sans-serif; max-width: 560px; margin: 0 auto; padding: 24px; border: 1px solid #e2e8f0; border-radius: 12px;'>
                <h2 style='color: #4f46e5; margin-top: 0;'>Password Reset Request</h2>
                <p>Hello {user.FirstName},</p>
                <p>We received a request to reset your password for your <strong>SkillHub</strong> account ({normalizedEmail}).</p>
                <div style='background: #f1f5f9; padding: 18px; border-radius: 8px; text-align: center; margin: 24px 0;'>
                    <span style='font-size: 32px; font-weight: bold; letter-spacing: 6px; color: #0f172a;'>{resetCode}</span>
                </div>
                <p style='color: #64748b; font-size: 13px;'>This 6-digit verification code will expire in <strong>15 minutes</strong>. If you did not request this, you can safely ignore this email.</p>
                <hr style='border: none; border-top: 1px solid #e2e8f0; margin: 20px 0;' />
                <p style='color: #94a3b8; font-size: 11px; margin-bottom: 0;'>SkillHub Learning Platform &bull; Secure Authentication Service</p>
            </div>";

        try
        {
            await emailService.SendEmailAsync(normalizedEmail, emailSubject, emailBody);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[EmailService] Failed to dispatch email: {ex.Message}");
        }

        return Ok(new { 
            message = "A 6-digit password reset verification code has been dispatched to your email address. It will expire in 15 minutes."
        });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto request, [FromServices] SkillHub.Infrastructure.Persistence.Context.ApplicationDbContext dbContext)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.ResetCode) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return BadRequest(new { message = "Email, verification code, and new password are required." });
        }

        if (request.NewPassword.Length < 6)
        {
            return BadRequest(new { message = "Password must be at least 6 characters long." });
        }

        var normalizedEmail = request.Email.Trim().ToLowerInvariant();
        if (!ResetCodeStore.TryGetValue(normalizedEmail, out var entry))
        {
            return BadRequest(new { message = "No reset request found for this email. Please request a new code." });
        }

        if (DateTime.UtcNow > entry.Expiry)
        {
            ResetCodeStore.TryRemove(normalizedEmail, out _);
            return BadRequest(new { message = "Reset code has expired. Please request a new code." });
        }

        if (entry.Code != request.ResetCode.Trim())
        {
            return BadRequest(new { message = "Invalid verification code. Please check and try again." });
        }

        var user = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions.FirstOrDefaultAsync(
            dbContext.Users, u => u.Email.ToLower() == normalizedEmail);

        if (user == null)
        {
            return NotFound(new { message = "User account not found." });
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        ResetCodeStore.TryRemove(normalizedEmail, out _);
        await dbContext.SaveChangesAsync();

        return Ok(new { message = "Your password has been successfully reset! You can now log in with your new password." });
    }

    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, (string Code, DateTime Expiry)> ResetCodeStore = new();
}

public class ForgotPasswordDto
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordDto
{
    public string Email { get; set; } = string.Empty;
    public string ResetCode { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}