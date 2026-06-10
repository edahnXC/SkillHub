using SkillHub.Domain.Common;

namespace SkillHub.Domain.Entities.Commerce
{
    public class Payment:BaseEntity
    {
        public long PaymentId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
