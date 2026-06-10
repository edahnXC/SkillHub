using SkillHub.Domain.Common;
namespace SkillHub.Domain.Entities.Assessments
{
    public class QuizAttempt:BaseEntity
    {
        public long QuizId { get; set; }
        public long UserId { get; set; }
        public decimal Score { get; set; }
    }
}
