using SkillHub.Domain.Common;

namespace SkillHub.Domain.Entities.Learning;

public class Enrollment : BaseEntity
{
    public long UserId { get; set; }
    public long CourseId { get; set; }

    public DateTime EnrollementDate { get; set; }
}