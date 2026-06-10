using SkillHub.Domain.Common;
namespace SkillHub.Domain.Entities.Assessments
{
    public class Quiz:BaseEntity
    {
        public long LessonId { get; set; }
        public string Title { get; set; } = string.Empty;
    }
}
