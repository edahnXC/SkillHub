using System;
using SkillHub.Domain.Common;

namespace SkillHub.Domain.Entities.Assessments
{
    public class QuizOption : BaseEntity
    {
        public long QuestionId { get; set; }
        public QuizQuestion Question { get; set; } = null!;

        public string OptionText { get; set; } = string.Empty;
        public bool IsCorrect { get; set; }
    }
}
