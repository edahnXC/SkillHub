using SkillHub.Application.Features.Courses.DTOs;

namespace SkillHub.Application.Features.Courses.Interfaces;

public interface ICourseService
{
    Task<CourseResponseDto> CreateAsync(
        CreateCourseRequestDto request, long instructorId);

    Task<List<CourseListDto>> GetAllAsync();

    Task<CourseResponseDto?> GetByIdAsync(
        long id);

    Task<List<CourseListDto>> GetCoursesByInstructorAsync(long instructorId);

    Task UpdateAsync(
        UpdateCourseRequestDto request, long instructorId);

    Task DeleteAsync(
        long id, long instructorId);

    Task<PlatformStatsDto> GetPlatformStatsAsync();
}
