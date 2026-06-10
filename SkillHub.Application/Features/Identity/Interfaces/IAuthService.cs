using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SkillHub.Application.Features.Identity.DTOs;
namespace SkillHub.Application.Features.Identity.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterRequestDto registerRequest);

        Task<AuthResponseDto>LoginAsync(LoginRequestDto loginRequest);
    }
}
//application layer need to be independent of the infrastructure layer and the domain layer
//and it needs authentication because it is the main feature of the application and it is
//used in many places in the application and it is also used in the presentation layer to
//authenticate the user and to authorize the user to access the resources and to perform
//the actions that are allowed for the user based on his role and permissions
