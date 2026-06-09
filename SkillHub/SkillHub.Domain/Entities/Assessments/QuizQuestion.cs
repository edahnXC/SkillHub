using SkillHub.SkillHub.Domain.Common;

namespace SkillHub.SkillHub.Domain.Entities.Assessments
{
    public class QuizQuestion:BaseEntity
    {
        public long QuizId { get; set; }
        public string QuestionText { get; set; } = string.Empty;
    }
}
