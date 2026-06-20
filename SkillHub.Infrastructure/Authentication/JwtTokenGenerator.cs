using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using SkillHub.Application.Features.Identity.Interfaces;
using SkillHub.Application.Features.Identity.Models;
using SkillHub.Domain.Entities.Identity;

namespace SkillHub.Infrastructure.Authentication;

public class JwtTokenGenerator
    : IJwtTokenGenerator
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(
        IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateToken(
        User user)
    {
        var claims =
            new List<Claim>
            {
                new(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new(
                    ClaimTypes.Email,
                    user.Email),

                new(
                    ClaimTypes.Name,
                    $"{user.FirstName} {user.LastName}")
            };

        var key =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    _jwtSettings.Key));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer:
                    _jwtSettings.Issuer,

                audience:
                    _jwtSettings.Audience,

                claims:
                    claims,

                expires:
                    DateTime.UtcNow.AddMinutes(
                        _jwtSettings.DurationInMinutes),

                signingCredentials:
                    credentials);

        return new JwtSecurityTokenHandler()
            .WriteToken(token);
    }
}

//explain why did we do this
