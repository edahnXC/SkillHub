using SkillHub.Domain.Common;

namespace SkillHub.Domain.Entities.Identity
{
    public class UserRole:BaseEntity
    {
        public long UserId { get; set; }

        public User User { get; set; }= null!; //userole is the bridge table between user
        //and role so it has a reference to both user and role

        public long RoleId { get; set; }

        public Role Role { get; set; } = null!;

    }
}
//one user can have many roles and one role can belong to many users
//many to many relationship and sql required a bridge table for it 