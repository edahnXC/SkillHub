using SkillHub.SkillHub.Domain.Common;

namespace SkillHub.SkillHub.Domain.Entities.Identity
{
    public class RolePermission:BaseEntity
    {
        public int RoleId { get; set; }

        public long PermissionId { get; set; }
    }
}
