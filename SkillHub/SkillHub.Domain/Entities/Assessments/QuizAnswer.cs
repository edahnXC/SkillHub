using SkillHub.SkillHub.Domain.Common;

namespace SkillHub.SkillHub.Domain.Entities.Assessments
{
    public class QuizAnswer:BaseEntity
    {
        public long AttemptId { get; set; }
        public long QuestionId { get; set; }
        public long SelectedOptionId { get; set; }
    }
}
