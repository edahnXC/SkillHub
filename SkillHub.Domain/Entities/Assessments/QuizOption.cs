using SkillHub.Domain.Common;
namespace SkillHub.Domain.Entities.Assessments
{
    public class QuizOption:BaseEntity
    {
        public long QuizQuestionId { get; set; }
        public string OptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }

    }
}
