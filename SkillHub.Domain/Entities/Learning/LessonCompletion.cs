using System;
using SkillHub.Domain.Common;
using SkillHub.Domain.Entities.Courses;
using SkillHub.Domain.Entities.Identity;

namespace SkillHub.Domain.Entities.Learning
{
    public class LessonCompletion : BaseEntity
    {
        public long StudentId { get; set; }
        public User Student { get; set; } = null!;

        public long CourseId { get; set; }
        public Course Course { get; set; } = null!;

        public long LessonId { get; set; }
        public CourseLesson Lesson { get; set; } = null!;

        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }
}
