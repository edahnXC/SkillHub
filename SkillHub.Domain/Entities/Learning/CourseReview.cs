using System;
using SkillHub.Domain.Common;
using SkillHub.Domain.Entities.Courses;
using SkillHub.Domain.Entities.Identity;

namespace SkillHub.Domain.Entities.Learning
{
    public class CourseReview : BaseEntity
    {
        public long CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public long StudentId { get; set; }
        public User Student { get; set; } = null!;

        public int Rating { get; set; }
        public string? ReviewText { get; set; }
    }
}
