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

                CertificateDto? certDto = null;
                if (progressPercentage >= 100 && totalLessons > 0)
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
                        CourseTitle = course.Title
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

            CertificateDto? certDto = null;
            if (isCourseNowCompleted)
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

                certDto = new CertificateDto
                {
                    CertificateNumber = cert.CertificateNumber,
                    IssuedAt = cert.IssuedAt,
                    StudentName = $"{enrollment.Student.FirstName} {enrollment.Student.LastName}",
                    CourseTitle = enrollment.Course.Title
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
                Certificate = certDto
            };
        }
    }
}
