using SkillHub.Application.Features.Courses.DTOs;

namespace SkillHub.Application.Features.Courses.Interfaces;

public interface IEnrollmentService
{
    Task<bool> EnrollStudentAsync(long studentId, long courseId);
    Task<List<CourseListDto>> GetStudentEnrollmentsAsync(long studentId);
    Task<bool> IsEnrolledAsync(long studentId, long courseId);
}
