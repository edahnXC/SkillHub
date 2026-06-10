using SkillHub.Domain.Common;

namespace SkillHub.Domain.Entities.Identity
{
    public class RolePermission:BaseEntity
    {
        public int RoleId { get; set; }

        public Role Role { get; set; } = null!;
        public long PermissionId { get; set; }

        public Permission Permission { get; set; }=null!;
    }
}
//explain why we are using a bridge table for role and permission
//and how they are used to assign permissions to roles and then roles to users