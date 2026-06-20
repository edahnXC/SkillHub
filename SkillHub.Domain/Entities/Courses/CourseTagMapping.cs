using SkillHub.Domain.Common;

namespace SkillHub.Domain.Entities.Courses;

public class CourseTagMapping:BaseEntity
    {
        public long CourseId { get; set; }
    public Course Course { get; set; }
    = null!;
        public long TagId { get; set; }
    public CourseTag Tag { get; set; }=null!;
    }

