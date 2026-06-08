using SkillHub.SkillHub.Domain.Common;

namespace SkillHub.SkillHub.Domain.Entities.Identity
{
    public class UserRole:BaseEntity
    {
        public long UserId { get; set; }

        public int RoleId { get; set; }
    }
}
//one user can have many roles and one role can belong to many users
//many to many relationship and sql required a bridge table for it 