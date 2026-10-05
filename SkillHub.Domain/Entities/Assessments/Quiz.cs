using System;
using System.Collections.Generic;
using SkillHub.Domain.Common;
using SkillHub.Domain.Entities.Courses;

namespace SkillHub.Domain.Entities.Assessments
{
    public class Quiz : BaseEntity
    {
        public long CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public long? SectionId { get; set; }
        public CourseSection? Section { get; set; }

        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int PassingScorePercentage { get; set; } = 70;

        public ICollection<QuizQuestion> Questions { get; set; } = new List<QuizQuestion>();
    }
}
