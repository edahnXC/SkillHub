using SkillHub.Domain.Common;

namespace SkillHub.Domain.Entities.Courses;

public class CourseTag : BaseEntity
{
    public string Name { get; set; } = string.Empty;
}
