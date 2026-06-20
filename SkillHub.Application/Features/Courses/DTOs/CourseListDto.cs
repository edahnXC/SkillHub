namespace SkillHub.Application.Features.Courses.DTOs;

public class CourseListDto
{
    public long Id { get; set; }

    public string Title { get; set; }
        = string.Empty;

    public decimal Price { get; set; }

    public string? ThumbnailUrl { get; set; }

    public bool IsPublished { get; set; }
}