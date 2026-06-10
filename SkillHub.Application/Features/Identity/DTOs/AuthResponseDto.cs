using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillHub.Application.Features.Identity.DTOs
{
    public class AuthResponseDto
    {
        public string Token { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
    }
}
//dtos expose only what is needed 
//so how is it decided what DTOs should have and what they should not have ?
//and what tpes of dtos should be created ?
//dtos are designed around use case and not around a database table
