using System;
using System.Collections.Generic;
using SkillHub.Domain.Common;
using SkillHub.Domain.Entities.Identity;

namespace SkillHub.Domain.Entities.Assessments
{
    public class QuizAttempt : BaseEntity
    {
        public long QuizId { get; set; }
        public Quiz Quiz { get; set; } = null!;

        public long StudentId { get; set; }
        public User Student { get; set; } = null!;

        public decimal ScorePercentage { get; set; }
        public bool IsPassed { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

        public ICollection<QuizAnswer> Answers { get; set; } = new List<QuizAnswer>();
    }
}
