using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Features.Courses.DTOs;
using SkillHub.Application.Features.Learning.DTOs;
using SkillHub.Application.Features.Learning.Interfaces;
using SkillHub.Domain.Entities.Learning;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Services.Learning
{
    public class LearningService : ILearningService
    {
        private readonly ApplicationDbContext _context;

        public LearningService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<CourseLearningDto> GetCourseLearningDataAsync(long userId, long courseId)
        {
            // 1. Check if user is enrolled
            var enrollment = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                    .ThenInclude(c => c.Instructor)
                .FirstOrDefaultAsync(e => e.StudentId == userId && e.CourseId == courseId);

            // 2. Fetch sections and lessons
            var sections = await _context.CourseSections
                .Where(s => s.CourseId == courseId)
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Id)
                .Include(s => s.Lessons)
                    .ThenInclude(l => l.Resources)
                .AsNoTracking()
                .ToListAsync();

            var totalLessons = sections.Sum(s => s.Lessons.Count);

            if (enrollment != null)
            {
                var course = enrollment.Course;
                var instructorName = course.Instructor != null 
                    ? $"{course.Instructor.FirstName} {course.Instructor.LastName}" 
                    : "Instructor";

                var completedLessonIds = await _context.LessonCompletions
                    .Where(lc => lc.StudentId == userId && lc.CourseId == courseId)
                    .Select(lc => lc.LessonId)
                    .ToListAsync();

                var completedCount = sections.Sum(s => s.Lessons.Count(l => completedLessonIds.Contains(l.Id)));
                var progressPercentage = totalLessons > 0 
                    ? Math.Round(((decimal)completedCount / totalLessons) * 100, 1) 
                    : 0;

                var (isCertEligible, certRequirementMessage, avgScore, certGrade, totalQuizzes, passedQuizzes) = 
                    await EvaluateCertificateEligibilityAsync(userId, courseId, completedCount, totalLessons);

                CertificateDto? certDto = null;
                if (isCertEligible)
                {
                    var cert = await _context.Certificates
                        .FirstOrDefaultAsync(c => c.StudentId == userId && c.CourseId == courseId);

                    if (cert == null)
                    {
                        var certCode = $"SKILLHUB-CERT-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
                        cert = new Certificate
                        {
                            StudentId = userId,
                            CourseId = courseId,
                            CertificateNumber = certCode,
                            IssuedAt = DateTime.UtcNow
                        };
                        _context.Certificates.Add(cert);
                        await _context.SaveChangesAsync();
                    }

                    certDto = new CertificateDto
                    {
                        CertificateNumber = cert.CertificateNumber,
                        IssuedAt = cert.IssuedAt,
                        StudentName = $"{enrollment.Student.FirstName} {enrollment.Student.LastName}",
                        CourseTitle = course.Title,
                        InstructorName = instructorName,
                        FinalScore = avgScore,
                        Grade = certGrade
                    };
                }

                return new CourseLearningDto
                {
                    CourseId = course.Id,
                    CourseTitle = course.Title,
                    ThumbnailUrl = course.ThumbnailUrl,
                    InstructorName = instructorName,
                    ProgressPercentage = progressPercentage,
                    CompletedLessonsCount = completedCount,
                    TotalLessonsCount = totalLessons,
                    Certificate = certDto,
                    IsCertificateEligible = isCertEligible,
                    CertificateRequirementMessage = certRequirementMessage,
                    TotalQuizzesCount = totalQuizzes,
                    PassedQuizzesCount = passedQuizzes,
                    AverageAssessmentScore = avgScore,
                    Sections = sections.Select(s => new LearningSectionDto
                    {
                        Id = s.Id,
                        Title = s.Title,
                        DisplayOrder = s.DisplayOrder,
                        Lessons = s.Lessons.OrderBy(l => l.Id).Select(l => new LearningLessonDto
                        {
                            Id = l.Id,
                            SectionId = s.Id,
                            Title = l.Title,
                            VideoUrl = l.VideoUrl,
                            DurationMinutes = l.DurationMinutes,
                            IsCompleted = completedLessonIds.Contains(l.Id),
                            Resources = l.Resources.Select(r => new LessonResourceDto
                            {
                                Id = r.Id,
                                FileName = r.FileName,
                                FileUrl = r.FileUrl
                            }).ToList()
                        }).ToList()
                    }).ToList()
                };
            }

            // 3. If not enrolled, allow SuperAdmin, SubAdmin, or course Instructor (Preview Mode)
            var user = await _context.Users
                .Include(u => u.UserRoles)
                    .ThenInclude(ur => ur.Role)
                .FirstOrDefaultAsync(u => u.Id == userId);

            var directCourse = await _context.Courses
                .Include(c => c.Instructor)
                .FirstOrDefaultAsync(c => c.Id == courseId);

            if (directCourse == null)
            {
                throw new KeyNotFoundException("Course not found.");
            }

            var roles = user?.UserRoles.Select(ur => ur.Role.Name).ToList() ?? new List<string>();
            bool isPrivileged = roles.Contains("SuperAdmin") || roles.Contains("SubAdmin") || directCourse.InstructorId == userId;

            if (!isPrivileged)
            {
                throw new UnauthorizedAccessException("You are not enrolled in this course.");
            }

            var previewInstructorName = directCourse.Instructor != null 
                ? $"{directCourse.Instructor.FirstName} {directCourse.Instructor.LastName}" 
                : "Instructor";

            return new CourseLearningDto
            {
                CourseId = directCourse.Id,
                CourseTitle = directCourse.Title,
                ThumbnailUrl = directCourse.ThumbnailUrl,
                InstructorName = previewInstructorName,
                ProgressPercentage = 0,
                CompletedLessonsCount = 0,
                TotalLessonsCount = totalLessons,
                Certificate = null,
                Sections = sections.Select(s => new LearningSectionDto
                {
                    Id = s.Id,
                    Title = s.Title,
                    DisplayOrder = s.DisplayOrder,
                    Lessons = s.Lessons.OrderBy(l => l.Id).Select(l => new LearningLessonDto
                    {
                        Id = l.Id,
                        SectionId = s.Id,
                        Title = l.Title,
                        VideoUrl = l.VideoUrl,
                        DurationMinutes = l.DurationMinutes,
                        IsCompleted = false,
                        Resources = l.Resources.Select(r => new LessonResourceDto
                        {
                            Id = r.Id,
                            FileName = r.FileName,
                            FileUrl = r.FileUrl
                        }).ToList()
                    }).ToList()
                }).ToList()
            };
        }

        public async Task<LessonCompletionResultDto> ToggleLessonCompletionAsync(long studentId, ToggleLessonCompletionRequestDto dto)
        {
            var enrollment = await _context.Enrollments
                .Include(e => e.Student)
                .Include(e => e.Course)
                .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == dto.CourseId);

            if (enrollment == null)
            {
                var userRoles = await _context.UserRoles
                    .Where(ur => ur.UserId == studentId)
                    .Select(ur => ur.Role.Name)
                    .ToListAsync();
                var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == dto.CourseId);
                bool isPrivileged = userRoles.Contains("SuperAdmin") || userRoles.Contains("SubAdmin") || (course != null && course.InstructorId == studentId);

                if (isPrivileged || (course != null && (course.Price == 0 || course.DiscountPrice == 0)))
                {
                    enrollment = new SkillHub.Domain.Entities.Courses.Enrollment
                    {
                        StudentId = studentId,
                        CourseId = dto.CourseId,
                        EnrolledAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Enrollments.Add(enrollment);
                    await _context.SaveChangesAsync();

                    enrollment = await _context.Enrollments
                        .Include(e => e.Student)
                        .Include(e => e.Course)
                        .FirstOrDefaultAsync(e => e.StudentId == studentId && e.CourseId == dto.CourseId);
                }
            }

            if (enrollment == null)
            {
                throw new UnauthorizedAccessException("You are not enrolled in this course.");
            }

            var existing = await _context.LessonCompletions
                .FirstOrDefaultAsync(lc => lc.StudentId == studentId && lc.LessonId == dto.LessonId && lc.CourseId == dto.CourseId);

            bool isNowCompleted;

            if (existing != null)
            {
                _context.LessonCompletions.Remove(existing);
                isNowCompleted = false;
            }
            else
            {
                var newCompletion = new LessonCompletion
                {
                    StudentId = studentId,
                    CourseId = dto.CourseId,
                    LessonId = dto.LessonId,
                    CompletedAt = DateTime.UtcNow
                };
                _context.LessonCompletions.Add(newCompletion);
                isNowCompleted = true;
            }

            await _context.SaveChangesAsync();

            var totalLessons = await _context.CourseSections
                .Where(s => s.CourseId == dto.CourseId)
                .SelectMany(s => s.Lessons)
                .CountAsync();

            var completedLessons = await _context.LessonCompletions
                .Where(lc => lc.StudentId == studentId && lc.CourseId == dto.CourseId)
                .CountAsync();

            var progressPercentage = totalLessons > 0 
                ? Math.Round(((decimal)completedLessons / totalLessons) * 100, 1) 
                : 0;

            bool isCourseNowCompleted = completedLessons >= totalLessons && totalLessons > 0;

            var (isCertEligible, certRequirementMessage, avgScore, certGrade, totalQuizzes, passedQuizzes) = 
                await EvaluateCertificateEligibilityAsync(studentId, dto.CourseId, completedLessons, totalLessons);

            CertificateDto? certDto = null;
            if (isCertEligible)
            {
                var cert = await _context.Certificates
                    .FirstOrDefaultAsync(c => c.StudentId == studentId && c.CourseId == dto.CourseId);

                if (cert == null)
                {
                    var certCode = $"SKILLHUB-CERT-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
                    cert = new Certificate
                    {
                        StudentId = studentId,
                        CourseId = dto.CourseId,
                        CertificateNumber = certCode,
                        IssuedAt = DateTime.UtcNow
                    };
                    _context.Certificates.Add(cert);
                    await _context.SaveChangesAsync();
                }

                var instructorName = enrollment.Course.Instructor != null 
                    ? $"{enrollment.Course.Instructor.FirstName} {enrollment.Course.Instructor.LastName}" 
                    : "Expert Instructor";

                certDto = new CertificateDto
                {
                    CertificateNumber = cert.CertificateNumber,
                    IssuedAt = cert.IssuedAt,
                    StudentName = $"{enrollment.Student.FirstName} {enrollment.Student.LastName}",
                    CourseTitle = enrollment.Course.Title,
                    InstructorName = instructorName,
                    FinalScore = avgScore,
                    Grade = certGrade
                };
            }

            return new LessonCompletionResultDto
            {
                LessonId = dto.LessonId,
                IsCompleted = isNowCompleted,
                UpdatedProgressPercentage = progressPercentage,
                CompletedLessonsCount = completedLessons,
                TotalLessonsCount = totalLessons,
                IsCourseCompleted = isCourseNowCompleted,
                Certificate = certDto,
                IsCertificateEligible = isCertEligible,
                CertificateRequirementMessage = certRequirementMessage
            };
        }

        private async Task<(bool isEligible, string message, decimal? avgScore, string grade, int totalQuizzes, int passedQuizzes)> EvaluateCertificateEligibilityAsync(long studentId, long courseId, int completedLessons, int totalLessons)
        {
            var quizzes = await _context.Quizzes
                .Where(q => q.CourseId == courseId)
                .ToListAsync();

            int totalQuizzes = quizzes.Count;
            int passedQuizzes = 0;
            decimal totalScore = 0;
            bool allQuizzesPassed = true;
            string pendingQuizReason = string.Empty;

            if (totalQuizzes > 0)
            {
                foreach (var quiz in quizzes)
                {
                    var bestAttempt = await _context.QuizAttempts
                        .Where(a => a.QuizId == quiz.Id && a.StudentId == studentId)
                        .OrderByDescending(a => a.ScorePercentage)
                        .FirstOrDefaultAsync();

                    int requiredScore = quiz.PassingScorePercentage > 0 ? quiz.PassingScorePercentage : 70;
                    if (bestAttempt != null && bestAttempt.ScorePercentage >= 70 && bestAttempt.ScorePercentage >= requiredScore)
                    {
                        passedQuizzes++;
                        totalScore += bestAttempt.ScorePercentage;
                    }
                    else
                    {
                        allQuizzesPassed = false;
                        if (bestAttempt == null)
                        {
                            pendingQuizReason = $"Mastery assessment '{quiz.Title}' must be completed with 70% or higher.";
                        }
                        else
                        {
                            pendingQuizReason = $"Assessment score is {bestAttempt.ScorePercentage}%. 70% or higher is required to unlock your certificate.";
                        }
                    }
                }
            }

            bool allLessonsCompleted = totalLessons > 0 && completedLessons >= totalLessons;

            if (!allLessonsCompleted)
            {
                return (false, $"Complete all lectures ({completedLessons}/{totalLessons} completed) to unlock your certificate.", null, string.Empty, totalQuizzes, passedQuizzes);
            }

            if (totalQuizzes > 0 && !allQuizzesPassed)
            {
                return (false, pendingQuizReason, null, string.Empty, totalQuizzes, passedQuizzes);
            }

            decimal? avgScore = totalQuizzes > 0 ? Math.Round(totalScore / totalQuizzes, 1) : 100m;
            string grade = avgScore >= 90 ? "Passed with Highest Distinction" : (avgScore >= 80 ? "Passed with Distinction" : "Passed (≥70%)");

            return (true, "Eligible for official certificate of completion.", avgScore, grade, totalQuizzes, passedQuizzes);
        }
    }
}
