using SkillHub.Domain.Common;

namespace SkillHub.Domain.Entities.Commerce
{
    public class Refund:BaseEntity
    {
        public long PaymentId { get; set; }
        public decimal RefundAmount { get; set; }

    }
}
