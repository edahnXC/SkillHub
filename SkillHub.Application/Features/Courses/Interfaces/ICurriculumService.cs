using System.Collections.Generic;
using System.Threading.Tasks;
using SkillHub.Application.Features.Courses.DTOs;

namespace SkillHub.Application.Features.Courses.Interfaces
{
    public interface ICurriculumService
    {
        Task<List<CourseSectionDto>> GetCurriculumByCourseIdAsync(long courseId);
        Task<CourseSectionDto> CreateSectionAsync(CreateSectionRequestDto dto, long instructorId);
        Task UpdateSectionAsync(UpdateSectionRequestDto dto, long instructorId);
        Task DeleteSectionAsync(long sectionId, long instructorId);
        Task<CourseLessonDto> CreateLessonAsync(CreateLessonRequestDto dto, long instructorId);
        Task UpdateLessonAsync(UpdateLessonRequestDto dto, long instructorId);
        Task DeleteLessonAsync(long lessonId, long instructorId);
        Task<LessonResourceDto> AddLessonResourceAsync(CreateLessonResourceRequestDto dto, long instructorId);
        Task DeleteLessonResourceAsync(long resourceId, long instructorId);
    }
}
