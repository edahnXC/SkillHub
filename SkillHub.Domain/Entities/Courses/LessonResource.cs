using SkillHub.Domain.Common;

namespace SkillHub.Domain.Entities.Courses;

public class LessonResource : BaseEntity
{
    public long LessonId { get; set; }
    public CourseLesson Lesson { get; set; } = null!;

    public string FileName { get; set; } = string.Empty;

    public string FileUrl { get; set; } = string.Empty;
}