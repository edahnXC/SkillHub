namespace SkillHub.Application.Features.Courses.DTOs;

public class CreateCourseRequestDto
{
    public string Title { get; set; }
        = string.Empty;

    public string Description { get; set; }
        = string.Empty;

    public decimal Price { get; set; }

    public decimal? DiscountPrice { get; set; }

    public string? ThumbnailUrl { get; set; }

    public long CategoryId { get; set; }
}