using System;
using System.Collections.Generic;
using SkillHub.Domain.Common;

namespace SkillHub.Domain.Entities.Assessments
{
    public class QuizQuestion : BaseEntity
    {
        public long QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;

        public string QuestionText { get; set; } = string.Empty;
        public string? Explanation { get; set; }
        public int DisplayOrder { get; set; }

        public ICollection<QuizOption> Options { get; set; } = new List<QuizOption>();
    }
}
