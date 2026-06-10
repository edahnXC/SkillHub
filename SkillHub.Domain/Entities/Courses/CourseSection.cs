using SkillHub.Domain.Common;

namespace SkillHub.Domain.Entities.Courses;

public class CourseSection : BaseEntity
{
    public long CourseId { get; set; }

    public string Title { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }
}

//remaining lesson and resource