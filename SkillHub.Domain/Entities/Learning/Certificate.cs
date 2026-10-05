using System;
using SkillHub.Domain.Common;
using SkillHub.Domain.Entities.Courses;
using SkillHub.Domain.Entities.Identity;

namespace SkillHub.Domain.Entities.Learning
{
    public class Certificate : BaseEntity
    {
        public long StudentId { get; set; }
        public User Student { get; set; } = null!;

        public long CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public string CertificateNumber { get; set; } = string.Empty;
        public DateTime IssuedAt { get; set; } = DateTime.UtcNow;
    }
}
