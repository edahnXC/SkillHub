using SkillHub.Domain.Common;
using SkillHub.Domain.Entities.Identity;

namespace SkillHub.Domain.Entities.Courses;

public class Course : AuditableEntity
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public decimal Price { get; set; }

    public decimal? DiscountPrice { get; set; }

    public string? ThumbnailUrl { get; set; }

    public bool IsPublished { get; set; }

    public long CategoryId { get; set; }

    public CourseCategory Category { get; set; }
        = null!;

    public long InstructorId { get; set; }

    public User Instructor { get; set; }
        = null!;

    public ICollection<CourseSection> Sections
    { get; set; }
        = new List<CourseSection>();

    public ICollection<CourseTagMapping> CourseTagMappings
    { get; set; }
        = new List<CourseTagMapping>();
}
//level 1 - coursecategory and coursetag have no dependencies 
//level 2 course - depends upon user and coursecategory
//level 3  - coursesection depends upon on course 
//level 4 - courselesson depends upon coursesection 
//level 5 - lessonresource depends courselesson
//level 6 - coursetapmapping depends on course, and coursetag
