using SkillHub.Domain.Common;

namespace SkillHub.Domain.Entities.Courses;

public class CourseSection : BaseEntity
{
    public long CourseId { get; set; }

    public Course Course { get; set; } = null!;

    public string Title { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public ICollection<CourseLesson> Lessons { get; set; }
    = new List<CourseLesson>();
}

//remaining lesson and resource