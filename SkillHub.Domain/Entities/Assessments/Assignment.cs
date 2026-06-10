using SkillHub.Domain.Common;
namespace SkillHub.Domain.Entities.Assessments
{
    public class Assignment:BaseEntity
    {
        public long CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
