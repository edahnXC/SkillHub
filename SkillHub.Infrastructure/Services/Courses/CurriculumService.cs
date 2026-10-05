using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Features.Courses.DTOs;
using SkillHub.Application.Features.Courses.Interfaces;
using SkillHub.Domain.Entities.Courses;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Services.Courses
{
    public class CurriculumService : ICurriculumService
    {
        private readonly ApplicationDbContext _context;

        public CurriculumService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<CourseSectionDto>> GetCurriculumByCourseIdAsync(long courseId)
        {
            var sections = await _context.CourseSections
                .Where(s => s.CourseId == courseId)
                .OrderBy(s => s.DisplayOrder)
                .ThenBy(s => s.Id)
                .Include(s => s.Lessons)
                    .ThenInclude(l => l.Resources)
                .AsNoTracking()
                .ToListAsync();

            return sections.Select(s => new CourseSectionDto
            {
                Id = s.Id,
                CourseId = s.CourseId,
                Title = s.Title,
                DisplayOrder = s.DisplayOrder,
                Lessons = s.Lessons
                    .OrderBy(l => l.Id)
                    .Select(l => new CourseLessonDto
                    {
                        Id = l.Id,
                        SectionId = l.SectionId,
                        Title = l.Title,
                        VideoUrl = l.VideoUrl,
                        DurationMinutes = l.DurationMinutes,
                        Resources = l.Resources.Select(r => new LessonResourceDto
                        {
                            Id = r.Id,
                            LessonId = r.LessonId,
                            FileName = r.FileName,
                            FileUrl = r.FileUrl
                        }).ToList()
                    }).ToList()
            }).ToList();
        }

        public async Task<CourseSectionDto> CreateSectionAsync(CreateSectionRequestDto dto, long instructorId)
        {
            var course = await _context.Courses.FirstOrDefaultAsync(c => c.Id == dto.CourseId);
            if (course == null)
            {
                throw new KeyNotFoundException("Course not found.");
            }

            if (course.InstructorId != instructorId)
            {
                throw new UnauthorizedAccessException("You are not authorized to modify this course.");
            }

            var section = new CourseSection
            {
                CourseId = dto.CourseId,
                Title = dto.Title,
                DisplayOrder = dto.DisplayOrder,
                CreatedAt = DateTime.UtcNow
            };

            _context.CourseSections.Add(section);
            await _context.SaveChangesAsync();

            return new CourseSectionDto
            {
                Id = section.Id,
                CourseId = section.CourseId,
                Title = section.Title,
                DisplayOrder = section.DisplayOrder,
                Lessons = new List<CourseLessonDto>()
            };
        }

        public async Task UpdateSectionAsync(UpdateSectionRequestDto dto, long instructorId)
        {
            var section = await _context.CourseSections
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == dto.Id);

            if (section == null)
            {
                throw new KeyNotFoundException("Section not found.");
            }

            if (section.Course.InstructorId != instructorId)
            {
                throw new UnauthorizedAccessException("You are not authorized to modify this course section.");
            }

            section.Title = dto.Title;
            section.DisplayOrder = dto.DisplayOrder;
            section.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteSectionAsync(long sectionId, long instructorId)
        {
            var section = await _context.CourseSections
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == sectionId);

            if (section == null)
            {
                throw new KeyNotFoundException("Section not found.");
            }

            if (section.Course.InstructorId != instructorId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this course section.");
            }

            _context.CourseSections.Remove(section);
            await _context.SaveChangesAsync();
        }

        public async Task<CourseLessonDto> CreateLessonAsync(CreateLessonRequestDto dto, long instructorId)
        {
            var section = await _context.CourseSections
                .Include(s => s.Course)
                .FirstOrDefaultAsync(s => s.Id == dto.SectionId);

            if (section == null)
            {
                throw new KeyNotFoundException("Section not found.");
            }

            if (section.Course.InstructorId != instructorId)
            {
                throw new UnauthorizedAccessException("You are not authorized to add lessons to this course.");
            }

            var lesson = new CourseLesson
            {
                SectionId = dto.SectionId,
                Title = dto.Title,
                VideoUrl = dto.VideoUrl,
                DurationMinutes = dto.DurationMinutes,
                CreatedAt = DateTime.UtcNow
            };

            _context.CourseLessons.Add(lesson);
            await _context.SaveChangesAsync();

            return new CourseLessonDto
            {
                Id = lesson.Id,
                SectionId = lesson.SectionId,
                Title = lesson.Title,
                VideoUrl = lesson.VideoUrl,
                DurationMinutes = lesson.DurationMinutes,
                Resources = new List<LessonResourceDto>()
            };
        }

        public async Task UpdateLessonAsync(UpdateLessonRequestDto dto, long instructorId)
        {
            var lesson = await _context.CourseLessons
                .Include(l => l.Section)
                    .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(l => l.Id == dto.Id);

            if (lesson == null)
            {
                throw new KeyNotFoundException("Lesson not found.");
            }

            if (lesson.Section.Course.InstructorId != instructorId)
            {
                throw new UnauthorizedAccessException("You are not authorized to modify this lesson.");
            }

            lesson.Title = dto.Title;
            lesson.VideoUrl = dto.VideoUrl;
            lesson.DurationMinutes = dto.DurationMinutes;
            lesson.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
        }

        public async Task DeleteLessonAsync(long lessonId, long instructorId)
        {
            var lesson = await _context.CourseLessons
                .Include(l => l.Section)
                    .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(l => l.Id == lessonId);

            if (lesson == null)
            {
                throw new KeyNotFoundException("Lesson not found.");
            }

            if (lesson.Section.Course.InstructorId != instructorId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this lesson.");
            }

            _context.CourseLessons.Remove(lesson);
            await _context.SaveChangesAsync();
        }

        public async Task<LessonResourceDto> AddLessonResourceAsync(CreateLessonResourceRequestDto dto, long instructorId)
        {
            var lesson = await _context.CourseLessons
                .Include(l => l.Section)
                    .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(l => l.Id == dto.LessonId);

            if (lesson == null)
            {
                throw new KeyNotFoundException("Lesson not found.");
            }

            if (lesson.Section.Course.InstructorId != instructorId)
            {
                throw new UnauthorizedAccessException("You are not authorized to attach resources to this lesson.");
            }

            var resource = new LessonResource
            {
                LessonId = dto.LessonId,
                FileName = dto.FileName,
                FileUrl = dto.FileUrl,
                CreatedAt = DateTime.UtcNow
            };

            _context.LessonResources.Add(resource);
            await _context.SaveChangesAsync();

            return new LessonResourceDto
            {
                Id = resource.Id,
                LessonId = resource.LessonId,
                FileName = resource.FileName,
                FileUrl = resource.FileUrl
            };
        }

        public async Task DeleteLessonResourceAsync(long resourceId, long instructorId)
        {
            var resource = await _context.LessonResources
                .Include(r => r.Lesson)
                    .ThenInclude(l => l.Section)
                        .ThenInclude(s => s.Course)
                .FirstOrDefaultAsync(r => r.Id == resourceId);

            if (resource == null)
            {
                throw new KeyNotFoundException("Resource not found.");
            }

            if (resource.Lesson.Section.Course.InstructorId != instructorId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this resource.");
            }

            _context.LessonResources.Remove(resource);
            await _context.SaveChangesAsync();
        }
    }
}
