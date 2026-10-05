using SkillHub.Domain.Common;

namespace SkillHub.Domain.Entities.Identity
{
    public class UserPermission : BaseEntity
    {
        public long UserId { get; set; }
        public User User { get; set; } = null!;

        public long PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;

        public bool IsGranted { get; set; } = true;
    }
}
