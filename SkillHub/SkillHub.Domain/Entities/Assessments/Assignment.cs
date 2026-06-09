using SkillHub.SkillHub.Domain.Common;
namespace SkillHub.SkillHub.Domain.Entities.Assessments
{
    public class Assignment
    {
        public long CourseId { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
