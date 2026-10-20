using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Features.Assessments.DTOs;
using SkillHub.Application.Features.Assessments.Interfaces;
using SkillHub.Domain.Entities.Assessments;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Services.Assessments
{
    public class QuizService : IQuizService
    {
        private readonly ApplicationDbContext _context;

        public QuizService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<QuizDto>> GetQuizzesByCourseIdAsync(long courseId, long? studentId = null)
        {
            var quizzes = await _context.Quizzes
                .Where(q => q.CourseId == courseId)
                .Include(q => q.Questions)
                    .ThenInclude(qu => qu.Options)
                .AsNoTracking()
                .ToListAsync();

            var result = new List<QuizDto>();

            foreach (var q in quizzes)
            {
                QuizAttemptResultDto? bestAttemptDto = null;
                if (studentId.HasValue)
                {
                    var bestAttempt = await _context.QuizAttempts
                        .Where(a => a.QuizId == q.Id && a.StudentId == studentId.Value)
                        .OrderByDescending(a => a.ScorePercentage)
                        .FirstOrDefaultAsync();

                    if (bestAttempt != null)
                    {
                        bestAttemptDto = new QuizAttemptResultDto
                        {
                            AttemptId = bestAttempt.Id,
                            QuizId = bestAttempt.QuizId,
                            ScorePercentage = bestAttempt.ScorePercentage,
                            IsPassed = bestAttempt.IsPassed,
                            PassingScorePercentage = q.PassingScorePercentage,
                            CompletedAt = bestAttempt.CompletedAt
                        };
                    }
                }

                result.Add(new QuizDto
                {
                    Id = q.Id,
                    CourseId = q.CourseId,
                    SectionId = q.SectionId,
                    Title = q.Title,
                    Description = q.Description,
                    PassingScorePercentage = q.PassingScorePercentage,
                    TotalQuestions = q.Questions.Count,
                    Questions = q.Questions
                        .OrderBy(qu => qu.DisplayOrder)
                        .Select(qu => new QuizQuestionDto
                        {
                            Id = qu.Id,
                            QuestionText = qu.QuestionText,
                            DisplayOrder = qu.DisplayOrder,
                            Options = qu.Options.Select(o => new QuizOptionDto
                            {
                                Id = o.Id,
                                OptionText = o.OptionText
                            }).ToList()
                        }).ToList(),
                    BestAttempt = bestAttemptDto
                });
            }

            return result;
        }

        public async Task<QuizDto?> GetQuizByIdAsync(long quizId, long? studentId = null)
        {
            var q = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qu => qu.Options)
                .AsNoTracking()
                .FirstOrDefaultAsync(q => q.Id == quizId);

            if (q == null) return null;

            QuizAttemptResultDto? bestAttemptDto = null;
            if (studentId.HasValue)
            {
                var bestAttempt = await _context.QuizAttempts
                    .Where(a => a.QuizId == q.Id && a.StudentId == studentId.Value)
                    .OrderByDescending(a => a.ScorePercentage)
                    .FirstOrDefaultAsync();

                if (bestAttempt != null)
                {
                    bestAttemptDto = new QuizAttemptResultDto
                    {
                        AttemptId = bestAttempt.Id,
                        QuizId = bestAttempt.QuizId,
                        ScorePercentage = bestAttempt.ScorePercentage,
                        IsPassed = bestAttempt.IsPassed,
                        PassingScorePercentage = q.PassingScorePercentage,
                        CompletedAt = bestAttempt.CompletedAt
                    };
                }
            }

            return new QuizDto
            {
                Id = q.Id,
                CourseId = q.CourseId,
                SectionId = q.SectionId,
                Title = q.Title,
                Description = q.Description,
                PassingScorePercentage = q.PassingScorePercentage,
                TotalQuestions = q.Questions.Count,
                Questions = q.Questions
                    .OrderBy(qu => qu.DisplayOrder)
                    .Select(qu => new QuizQuestionDto
                    {
                        Id = qu.Id,
                        QuestionText = qu.QuestionText,
                        DisplayOrder = qu.DisplayOrder,
                        Options = qu.Options.Select(o => new QuizOptionDto
                        {
                            Id = o.Id,
                            OptionText = o.OptionText
                        }).ToList()
                    }).ToList(),
                BestAttempt = bestAttemptDto
            };
        }

        public async Task<QuizAttemptResultDto> SubmitQuizAttemptAsync(long studentId, SubmitQuizAttemptRequestDto dto)
        {
            var quiz = await _context.Quizzes
                .Include(q => q.Questions)
                    .ThenInclude(qu => qu.Options)
                .FirstOrDefaultAsync(q => q.Id == dto.QuizId);

            if (quiz == null)
            {
                throw new KeyNotFoundException("Quiz not found.");
            }

            // Verify student is enrolled in the course
            var isEnrolled = await _context.Enrollments
                .AnyAsync(e => e.StudentId == studentId && e.CourseId == quiz.CourseId);

            if (!isEnrolled)
            {
                var userRoles = await _context.UserRoles
                    .Where(ur => ur.UserId == studentId)
                    .Select(ur => ur.Role.Name)
                    .ToListAsync();
                var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == quiz.CourseId);
                bool isPrivileged = userRoles.Contains("SuperAdmin") || userRoles.Contains("SubAdmin") || (course != null && course.InstructorId == studentId);

                if (isPrivileged || (course != null && (course.Price == 0 || course.DiscountPrice == 0)))
                {
                    var autoEnrollment = new SkillHub.Domain.Entities.Courses.Enrollment
                    {
                        StudentId = studentId,
                        CourseId = quiz.CourseId,
                        EnrolledAt = DateTime.UtcNow,
                        CreatedAt = DateTime.UtcNow
                    };
                    _context.Enrollments.Add(autoEnrollment);
                    await _context.SaveChangesAsync();
                    isEnrolled = true;
                }
            }

            if (!isEnrolled)
            {
                throw new UnauthorizedAccessException("You must be enrolled in this course to take this quiz.");
            }

            int correctCount = 0;
            var totalQuestions = quiz.Questions.Count;
            var feedbackList = new List<QuestionFeedbackDto>();
            var attemptAnswers = new List<QuizAnswer>();

            var attempt = new QuizAttempt
            {
                QuizId = quiz.Id,
                StudentId = studentId,
                CompletedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            foreach (var q in quiz.Questions)
            {
                var correctOption = q.Options.FirstOrDefault(o => o.IsCorrect);
                var correctOptionId = correctOption?.Id ?? 0;

                var studentAnswer = dto.Answers.FirstOrDefault(a => a.QuestionId == q.Id);
                var selectedOptionId = studentAnswer?.SelectedOptionId ?? 0;

                var isCorrect = selectedOptionId > 0 && selectedOptionId == correctOptionId;
                if (isCorrect)
                {
                    correctCount++;
                }

                feedbackList.Add(new QuestionFeedbackDto
                {
                    QuestionId = q.Id,
                    QuestionText = q.QuestionText,
                    SelectedOptionId = selectedOptionId,
                    CorrectOptionId = correctOptionId,
                    IsCorrect = isCorrect,
                    Explanation = q.Explanation
                });

                if (selectedOptionId > 0)
                {
                    attemptAnswers.Add(new QuizAnswer
                    {
                        QuestionId = q.Id,
                        SelectedOptionId = selectedOptionId,
                        IsCorrect = isCorrect,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }

            decimal scorePercentage = totalQuestions > 0
                ? Math.Round(((decimal)correctCount / totalQuestions) * 100, 1)
                : 0;

            bool isPassed = scorePercentage >= quiz.PassingScorePercentage;

            attempt.ScorePercentage = scorePercentage;
            attempt.IsPassed = isPassed;
            attempt.Answers = attemptAnswers;

            _context.QuizAttempts.Add(attempt);
            await _context.SaveChangesAsync();

            bool isCertificateAwarded = false;
            SkillHub.Application.Features.Learning.DTOs.CertificateDto? awardedCertDto = null;

            if (scorePercentage >= 70 && isPassed)
            {
                var totalLessons = await _context.CourseSections
                    .Where(s => s.CourseId == quiz.CourseId)
                    .SelectMany(s => s.Lessons)
                    .CountAsync();

                var completedLessons = await _context.LessonCompletions
                    .Where(lc => lc.StudentId == studentId && lc.CourseId == quiz.CourseId)
                    .CountAsync();

                if (totalLessons > 0 && completedLessons >= totalLessons)
                {
                    var courseQuizzes = await _context.Quizzes
                        .Where(q => q.CourseId == quiz.CourseId)
                        .ToListAsync();

                    bool allPassed = true;
                    decimal sumScore = 0;
                    foreach (var q in courseQuizzes)
                    {
                        if (q.Id == quiz.Id)
                        {
                            sumScore += scorePercentage;
                            continue;
                        }

                        var bestOther = await _context.QuizAttempts
                            .Where(a => a.QuizId == q.Id && a.StudentId == studentId)
                            .OrderByDescending(a => a.ScorePercentage)
                            .FirstOrDefaultAsync();

                        if (bestOther == null || bestOther.ScorePercentage < 70)
                        {
                            allPassed = false;
                            break;
                        }
                        sumScore += bestOther.ScorePercentage;
                    }

                    if (allPassed)
                    {
                        var cert = await _context.Certificates
                            .FirstOrDefaultAsync(c => c.StudentId == studentId && c.CourseId == quiz.CourseId);

                        if (cert == null)
                        {
                            var certCode = $"SKILLHUB-CERT-{Guid.NewGuid().ToString("N")[..8].ToUpper()}";
                            cert = new SkillHub.Domain.Entities.Learning.Certificate
                            {
                                StudentId = studentId,
                                CourseId = quiz.CourseId,
                                CertificateNumber = certCode,
                                IssuedAt = DateTime.UtcNow
                            };
                            _context.Certificates.Add(cert);
                            await _context.SaveChangesAsync();
                        }

                        var user = await _context.Users.FindAsync(studentId);
                        var course = await _context.Courses.Include(c => c.Instructor).FirstOrDefaultAsync(c => c.Id == quiz.CourseId);
                        var instructorName = course?.Instructor != null 
                            ? $"{course.Instructor.FirstName} {course.Instructor.LastName}" 
                            : "Expert Instructor";

                        decimal avgScore = courseQuizzes.Count > 0 ? Math.Round(sumScore / courseQuizzes.Count, 1) : scorePercentage;
                        string grade = avgScore >= 90 ? "Passed with Highest Distinction" : (avgScore >= 80 ? "Passed with Distinction" : "Passed (≥70%)");

                        awardedCertDto = new SkillHub.Application.Features.Learning.DTOs.CertificateDto
                        {
                            CertificateNumber = cert.CertificateNumber,
                            IssuedAt = cert.IssuedAt,
                            StudentName = user != null ? $"{user.FirstName} {user.LastName}" : "Student",
                            CourseTitle = course?.Title ?? "SkillHub Course",
                            InstructorName = instructorName,
                            FinalScore = avgScore,
                            Grade = grade
                        };
                        isCertificateAwarded = true;
                    }
                }
            }

            return new QuizAttemptResultDto
            {
                AttemptId = attempt.Id,
                QuizId = quiz.Id,
                ScorePercentage = scorePercentage,
                CorrectCount = correctCount,
                TotalQuestions = totalQuestions,
                IsPassed = isPassed,
                PassingScorePercentage = quiz.PassingScorePercentage,
                CompletedAt = attempt.CompletedAt,
                QuestionFeedback = feedbackList,
                IsCertificateAwarded = isCertificateAwarded,
                Certificate = awardedCertDto
            };
        }

        public async Task<QuizDto> CreateQuizAsync(long instructorId, CreateQuizRequestDto dto)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == dto.CourseId);
            if (course == null)
            {
                throw new KeyNotFoundException("Course not found.");
            }

            if (course.InstructorId != instructorId)
            {
                var userRoles = await _context.UserRoles
                    .Where(ur => ur.UserId == instructorId)
                    .Select(ur => ur.Role.Name)
                    .ToListAsync();
                if (!userRoles.Contains("SuperAdmin") && !userRoles.Contains("SubAdmin"))
                {
                    throw new UnauthorizedAccessException("You are not authorized to add quizzes to this course.");
                }
            }

            var quiz = new Quiz
            {
                CourseId = dto.CourseId,
                SectionId = dto.SectionId,
                Title = dto.Title,
                Description = dto.Description,
                PassingScorePercentage = dto.PassingScorePercentage,
                CreatedAt = DateTime.UtcNow,
                Questions = dto.Questions.Select(q => new QuizQuestion
                {
                    QuestionText = q.QuestionText,
                    Explanation = q.Explanation,
                    DisplayOrder = q.DisplayOrder,
                    CreatedAt = DateTime.UtcNow,
                    Options = q.Options.Select(o => new QuizOption
                    {
                        OptionText = o.OptionText,
                        IsCorrect = o.IsCorrect,
                        CreatedAt = DateTime.UtcNow
                    }).ToList()
                }).ToList()
            };

            _context.Quizzes.Add(quiz);
            await _context.SaveChangesAsync();

            return new QuizDto
            {
                Id = quiz.Id,
                CourseId = quiz.CourseId,
                SectionId = quiz.SectionId,
                Title = quiz.Title,
                Description = quiz.Description,
                PassingScorePercentage = quiz.PassingScorePercentage,
                TotalQuestions = quiz.Questions.Count,
                Questions = quiz.Questions.Select(qu => new QuizQuestionDto
                {
                    Id = qu.Id,
                    QuestionText = qu.QuestionText,
                    DisplayOrder = qu.DisplayOrder,
                    Options = qu.Options.Select(o => new QuizOptionDto
                    {
                        Id = o.Id,
                        OptionText = o.OptionText
                    }).ToList()
                }).ToList()
            };
        }
    }
}
