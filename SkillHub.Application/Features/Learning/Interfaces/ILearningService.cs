using System.Threading.Tasks;
using SkillHub.Application.Features.Learning.DTOs;

namespace SkillHub.Application.Features.Learning.Interfaces
{
    public interface ILearningService
    {
        Task<CourseLearningDto> GetCourseLearningDataAsync(long studentId, long courseId);
        Task<LessonCompletionResultDto> ToggleLessonCompletionAsync(long studentId, ToggleLessonCompletionRequestDto dto);
    }
}
