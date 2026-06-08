using SkillHub.SkillHub.Domain.Common;

namespace SkillHub.SkillHub.Domain.Entities.Identity
{
    public class Role:BaseEntity
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
