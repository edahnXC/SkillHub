using SkillHub.Domain.Common;

namespace SkillHub.Domain.Entities.Courses;

public class CourseLesson : BaseEntity
{
    public long SectionId { get; set; }
    public CourseSection Section { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public string? VideoUrl { get; set; }

    public int DurationMinutes { get; set; }
    public ICollection<LessonResource> Resources { get; set; }=new List<LessonResource>();
}