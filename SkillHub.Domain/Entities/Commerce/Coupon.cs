using SkillHub.Domain.Common;

namespace SkillHub.Domain.Entities.Commerce
{
    public class Coupon:BaseEntity
    {
        public string Code { get; set; } = string.Empty;
        public decimal DiscountAmount { get; set; }
    }
}
