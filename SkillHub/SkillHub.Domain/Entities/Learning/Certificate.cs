using SkillHub.SkillHub.Domain.Common

namespace SkillHub.SkillHub.Domain.Entities.Learning
{
    public class Certificate:BaseEntity
    {
        public long UserId { get; set; }
        public long CourseId { get; set; }
        public string CertificateNumber { get; set; } = string.Empty;
    }
}
