using SkillHub.SkillHub.Domain.Common;

namespace SkillHub.SkillHub.Domain.Entities.Commerce
{
    public class Order:BaseEntity
    {
        public long UserId { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
