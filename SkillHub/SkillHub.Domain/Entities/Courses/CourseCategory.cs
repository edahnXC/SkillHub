using SkillHub.SkillHub.Domain.Common;

namespace SkillHub.SkillHub.Domain.Entities.Courses;

public class CourseCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
}