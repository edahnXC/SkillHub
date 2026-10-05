using System;
using System.Collections.Generic;

namespace SkillHub.Application.Features.Reviews.DTOs
{
    public class CourseReviewDto
    {
        public long Id { get; set; }
        public long CourseId { get; set; }
        public long StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string? StudentAvatarUrl { get; set; }
        public int Rating { get; set; }
        public string? ReviewText { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class CourseReviewSummaryDto
    {
        public decimal AverageRating { get; set; }
        public int TotalReviews { get; set; }
        public Dictionary<int, int> RatingCounts { get; set; } = new();
        public Dictionary<int, decimal> RatingPercentages { get; set; } = new();
        public List<CourseReviewDto> Reviews { get; set; } = new();
        public CourseReviewDto? UserReview { get; set; }
    }

    public class CreateOrUpdateReviewRequestDto
    {
        public long CourseId { get; set; }
        public int Rating { get; set; }
        public string? ReviewText { get; set; }
    }
}
