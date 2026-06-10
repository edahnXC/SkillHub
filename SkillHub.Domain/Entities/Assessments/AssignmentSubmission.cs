using SkillHub.Domain.Common;
namespace SkillHub.Domain.Entities.Assessments
{
    public class AssignmentSubmission:BaseEntity
    {
        public long AssignmentId { get; set; }
        public long UserId { get; set; }
        public string FileUrl { get; set; } = string.Empty;
    }
}
