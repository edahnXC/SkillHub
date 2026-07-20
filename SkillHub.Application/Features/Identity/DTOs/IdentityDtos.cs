using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillHub.Application.Features.Identity.DTOs
{
    public class IdentityDtos
    {
    }
    public class LoginRequestDto
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
    public class RegisterRequestDto
    {
        public string FirstName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string ConfirmPassword { get; set; } = string.Empty;
    }
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public List<string> Roles { get; set; } = new();
    }
    public class InstructorDto
    {
        public long Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? ProfileImageUrl { get; set; }
        public int CourseCount { get; set; }
    }
    public class RefreshTokenRequestDto
    {
        public string Token { get; set; } = string.Empty;
    }
}
//dtos expose only what is needed 
//so how is it decided what DTOs should have and what they should not have ?
//and what tpes of dtos should be created ?
//dtos are designed around use case and not around a database table

