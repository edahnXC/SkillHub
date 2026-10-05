using System;
using System.Collections.Generic;

namespace SkillHub.Application.Features.Courses.DTOs
{
    public class CourseSectionDto
    {
        public long Id { get; set; }
        public long CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
        public List<CourseLessonDto> Lessons { get; set; } = new();
    }

    public class CourseLessonDto
    {
        public long Id { get; set; }
        public long SectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? VideoUrl { get; set; }
        public int DurationMinutes { get; set; }
        public List<LessonResourceDto> Resources { get; set; } = new();
    }

    public class LessonResourceDto
    {
        public long Id { get; set; }
        public long LessonId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
    }

    public class CreateSectionRequestDto
    {
        public long CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    public class UpdateSectionRequestDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int DisplayOrder { get; set; }
    }

    public class CreateLessonRequestDto
    {
        public long SectionId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? VideoUrl { get; set; }
        public int DurationMinutes { get; set; }
    }

    public class UpdateLessonRequestDto
    {
        public long Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? VideoUrl { get; set; }
        public int DurationMinutes { get; set; }
    }

    public class CreateLessonResourceRequestDto
    {
        public long LessonId { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FileUrl { get; set; } = string.Empty;
    }
}
