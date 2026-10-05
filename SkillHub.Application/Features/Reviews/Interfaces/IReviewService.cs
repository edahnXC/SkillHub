using System.Threading.Tasks;
using SkillHub.Application.Features.Reviews.DTOs;

namespace SkillHub.Application.Features.Reviews.Interfaces
{
    public interface IReviewService
    {
        Task<CourseReviewSummaryDto> GetCourseReviewsAsync(long courseId, long? currentUserId = null);
        Task<CourseReviewDto> SubmitReviewAsync(long studentId, CreateOrUpdateReviewRequestDto dto);
        Task DeleteReviewAsync(long reviewId, long currentUserId, bool isAdmin);
    }
}
