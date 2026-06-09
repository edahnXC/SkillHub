using SkillHub.SkillHub.Domain.Common;

namespace SkillHub.SkillHub.Domain.Entities.Assessments
{
    public class AssignmentSubmission
    {
        public long AssignmentId { get; set; }
        public long UserId { get; set; }
        public string FileUrl { get; set; } = string.Empty;
    }
}
