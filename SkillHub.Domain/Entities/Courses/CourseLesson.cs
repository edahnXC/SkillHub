using SkillHub.Domain.Common;

namespace SkillHub.Domain.Entities.Courses;

public class CourseLesson : BaseEntity
{
    public long SectionId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? VideoUrl { get; set; }

    public int DurationMinutes { get; set; }
}