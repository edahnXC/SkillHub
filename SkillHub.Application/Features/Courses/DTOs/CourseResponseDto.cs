namespace SkillHub.Application.Features.Courses.DTOs;

public class CourseResponseDto
{
    public long Id { get; set; }

    public string Title { get; set; }
        = string.Empty;

    public string Description { get; set; }
        = string.Empty;

    public decimal Price { get; set; }

    public decimal? DiscountPrice { get; set; }

    public string? ThumbnailUrl { get; set; }

    public bool IsPublished { get; set; }

    public string CategoryName { get; set; }
        = string.Empty;

    public string InstructorName { get; set; }
        = string.Empty;
}