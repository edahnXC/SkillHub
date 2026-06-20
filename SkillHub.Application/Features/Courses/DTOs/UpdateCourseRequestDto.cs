namespace SkillHub.Application.Features.Courses.DTOs;

public class UpdateCourseRequestDto
{
    public long Id { get; set; }

    public string Title { get; set; }
        = string.Empty;

    public string Description { get; set; }
        = string.Empty;

    public decimal Price { get; set; }

    public decimal? DiscountPrice { get; set; }

    public string? ThumbnailUrl { get; set; }

    public long CategoryId { get; set; }

    public bool IsPublished { get; set; }
}