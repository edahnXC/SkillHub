using System.Collections.Generic;
using System.Threading.Tasks;
using SkillHub.Application.Features.Assessments.DTOs;

namespace SkillHub.Application.Features.Assessments.Interfaces
{
    public interface IQuizService
    {
        Task<List<QuizDto>> GetQuizzesByCourseIdAsync(long courseId, long? studentId = null);
        Task<QuizDto?> GetQuizByIdAsync(long quizId, long? studentId = null);
        Task<QuizAttemptResultDto> SubmitQuizAttemptAsync(long studentId, SubmitQuizAttemptRequestDto dto);
        Task<QuizDto> CreateQuizAsync(long instructorId, CreateQuizRequestDto dto);
    }
}
