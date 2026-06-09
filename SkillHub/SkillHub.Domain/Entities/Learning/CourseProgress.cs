using SkillHub.SkillHub.Domain.Common;

namespace SkillHub.SkillHub.Domain.Entities.Learning;

public class CourseProgress:BaseEntity
{
    public long EnrollmentId { get; set; }

    public decimal CompletionPercentage { get; set; }
}
