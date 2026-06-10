using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillHub.Application.Features.Identity.DTOs;

    public class RegisterRequestDto
    {
    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; }= string.Empty;
    public string Email { get;set;  } = string.Empty;
    public string Password {  get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    }

