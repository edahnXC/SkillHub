using System;
using SkillHub.Domain.Common;

namespace SkillHub.Domain.Entities.Assessments
{
    public class QuizAnswer : BaseEntity
    {
        public long AttemptId { get; set; }
        public QuizAttempt Attempt { get; set; } = null!;

        public long QuestionId { get; set; }
        public QuizQuestion Question { get; set; } = null!;

        public long SelectedOptionId { get; set; }
        public QuizOption SelectedOption { get; set; } = null!;

        public bool IsCorrect { get; set; }
    }
}
