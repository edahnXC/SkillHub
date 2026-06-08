using SkillHub.SkillHub.Domain.Common;
namespace SkillHub.SkillHub.Domain.Entities.Courses
{
    public class CourseTagMapping:BaseEntity
    {
        public long CourseId { get; set; }
        public long TagId { get; set; }
    }
}
