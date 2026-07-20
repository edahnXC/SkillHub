using SkillHub.Domain.Common;
using SkillHub.Domain.Entities.Identity;

namespace SkillHub.Domain.Entities.Courses;

public class Enrollment : AuditableEntity
{
    public long StudentId { get; set; }
    public User Student { get; set; } = null!;

    public long CourseId { get; set; }
    public Course Course { get; set; } = null!;

    public DateTime EnrolledAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}
