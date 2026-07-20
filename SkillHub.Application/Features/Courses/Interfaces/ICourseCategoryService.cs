using SkillHub.Application.Features.Courses.DTOs;

namespace SkillHub.Application.Features.Courses.Interfaces;

public interface ICourseCategoryService
{
    Task<IEnumerable<CourseCategoryDto>> GetAllCategoriesAsync();
}
