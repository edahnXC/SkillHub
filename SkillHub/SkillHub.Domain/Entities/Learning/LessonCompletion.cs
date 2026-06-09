using SkillHub.SkillHub.Domain.Common;

namespace SkillHub.SkillHub.Domain.Entities.Learning;

public class LessonCompletion : BaseEntity
{
    public long Enrolment { get; set; }
    public long LessonId { get; set; }
    public DateTime CompletedAt { get; set; }
}