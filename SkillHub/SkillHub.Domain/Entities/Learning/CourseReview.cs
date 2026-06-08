using SkillHub.SkillHub.Domain.Common;

namespace SkillHub.SkillHub.Domain.Entities.Learning;

public class CourseReview : BaseEntity
{
    public long CourseId { get; set; }

    public long UserId { get; set; }

    public int Rating { get; set; }

    public string? ReviewText { get; set; }
}