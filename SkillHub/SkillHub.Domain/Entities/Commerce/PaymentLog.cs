using SkillHub.SkillHub.Domain.Common;

namespace SkillHub.SkillHub.Domain.Entities.Commerce
{
    public class PaymentLog:BaseEntity
    {
        public long PaymentId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
