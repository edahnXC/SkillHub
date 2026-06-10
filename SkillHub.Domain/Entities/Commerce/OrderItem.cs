using SkillHub.Domain.Common;

namespace SkillHub.Domain.Entities.Commerce
{
    public class OrderItem:BaseEntity
    {
        public long OrderId { get; set; }
        public long CourseId { get; set; }
        public decimal Price { get; set; }
    }
}
