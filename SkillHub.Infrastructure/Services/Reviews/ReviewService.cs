using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Features.Reviews.DTOs;
using SkillHub.Application.Features.Reviews.Interfaces;
using SkillHub.Domain.Entities.Learning;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Services.Reviews
{
    public class ReviewService : IReviewService
    {
        private readonly ApplicationDbContext _context;

        public ReviewService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CourseReviewSummaryDto> GetCourseReviewsAsync(long courseId, long? currentUserId = null)
        {
            var reviews = await _context.CourseReviews
                .Where(r => r.CourseId == courseId)
                .Include(r => r.Student)
                .OrderByDescending(r => r.CreatedAt)
                .AsNoTracking()
                .ToListAsync();

            var totalReviews = reviews.Count;
            var averageRating = totalReviews > 0
                ? Math.Round((decimal)reviews.Average(r => r.Rating), 1)
                : 0;

            var ratingCounts = new Dictionary<int, int>
            {
                { 5, 0 }, { 4, 0 }, { 3, 0 }, { 2, 0 }, { 1, 0 }
            };

            var ratingPercentages = new Dictionary<int, decimal>
            {
                { 5, 0 }, { 4, 0 }, { 3, 0 }, { 2, 0 }, { 1, 0 }
            };

            foreach (var r in reviews)
            {
                if (ratingCounts.ContainsKey(r.Rating))
                {
                    ratingCounts[r.Rating]++;
                }
            }

            if (totalReviews > 0)
            {
                for (int star = 1; star <= 5; star++)
                {
                    ratingPercentages[star] = Math.Round(((decimal)ratingCounts[star] / totalReviews) * 100, 1);
                }
            }

            var reviewDtos = reviews.Select(r => new CourseReviewDto
            {
                Id = r.Id,
                CourseId = r.CourseId,
                StudentId = r.StudentId,
                StudentName = $"{r.Student.FirstName} {r.Student.LastName}",
                Rating = r.Rating,
                ReviewText = r.ReviewText,
                CreatedAt = r.CreatedAt
            }).ToList();

            CourseReviewDto? userReviewDto = null;
            if (currentUserId.HasValue)
            {
                userReviewDto = reviewDtos.FirstOrDefault(r => r.StudentId == currentUserId.Value);
            }

            return new CourseReviewSummaryDto
            {
                AverageRating = averageRating,
                TotalReviews = totalReviews,
                RatingCounts = ratingCounts,
                RatingPercentages = ratingPercentages,
                Reviews = reviewDtos,
                UserReview = userReviewDto
            };
        }

        public async Task<CourseReviewDto> SubmitReviewAsync(long studentId, CreateOrUpdateReviewRequestDto dto)
        {
            if (dto.Rating < 1 || dto.Rating > 5)
            {
                throw new ArgumentException("Rating must be between 1 and 5 stars.");
            }

            // Verify student is enrolled
            var isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.CourseId == dto.CourseId);

            if (!isEnrolled)
            {
                throw new UnauthorizedAccessException("You must be enrolled in this course to leave a review.");
            }

            var existingReview = await _context.CourseReviews
                .Include(r => r.Student)
                .FirstOrDefaultAsync(r => r.StudentId == studentId && r.CourseId == dto.CourseId);

            if (existingReview != null)
            {
                existingReview.Rating = dto.Rating;
                existingReview.ReviewText = dto.ReviewText;
                existingReview.UpdatedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync();

                return new CourseReviewDto
                {
                    Id = existingReview.Id,
                    CourseId = existingReview.CourseId,
                    StudentId = existingReview.StudentId,
                    StudentName = $"{existingReview.Student.FirstName} {existingReview.Student.LastName}",
                    Rating = existingReview.Rating,
                    ReviewText = existingReview.ReviewText,
                    CreatedAt = existingReview.CreatedAt
                };
            }
            else
            {
                var newReview = new CourseReview
                {
                    StudentId = studentId,
                    CourseId = dto.CourseId,
                    Rating = dto.Rating,
                    ReviewText = dto.ReviewText,
                    CreatedAt = DateTime.UtcNow
                };

                _context.CourseReviews.Add(newReview);
                await _context.SaveChangesAsync();

                var student = await _context.Users.FindAsync(studentId);

                return new CourseReviewDto
                {
                    Id = newReview.Id,
                    CourseId = newReview.CourseId,
                    StudentId = newReview.StudentId,
                    StudentName = student != null ? $"{student.FirstName} {student.LastName}" : "Student",
                    Rating = newReview.Rating,
                    ReviewText = newReview.ReviewText,
                    CreatedAt = newReview.CreatedAt
                };
            }
        }

        public async Task DeleteReviewAsync(long reviewId, long currentUserId, bool isAdmin)
        {
            var review = await _context.CourseReviews.FindAsync(reviewId);
            if (review == null)
            {
                throw new KeyNotFoundException("Review not found.");
            }

            if (review.StudentId != currentUserId && !isAdmin)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this review.");
            }

            _context.CourseReviews.Remove(review);
            await _context.SaveChangesAsync();
        }
    }
}
