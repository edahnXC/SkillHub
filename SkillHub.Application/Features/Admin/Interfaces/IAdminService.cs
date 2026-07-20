using SkillHub.Application.Features.Admin.DTOs;

namespace SkillHub.Application.Features.Admin.Interfaces;

public interface IAdminService
{
    Task<IEnumerable<AdminUserDto>> GetAllUsersAsync();
    Task DeleteUserAsync(long id);
    Task CreateUserAsync(CreateUserRequestDto dto);

    Task<IEnumerable<AdminCourseDto>> GetAllCoursesAsync();
    Task DeleteCourseAsync(long id);
    Task ApproveCourseAsync(long id);
}
