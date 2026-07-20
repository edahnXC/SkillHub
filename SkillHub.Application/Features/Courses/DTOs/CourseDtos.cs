using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SkillHub.Application.Features.Courses.DTOs
{
    public class CourseDtos
    {
    }
    public class CourseCategoryDto
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class CourseListDto
    {
        public long Id { get; set; }

        public string Title { get; set; }
            = string.Empty;

        public decimal Price { get; set; }

        public string? ThumbnailUrl { get; set; }

        public bool IsPublished { get; set; }

        public long InstructorId { get; set; }
    }

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

        public long InstructorId { get; set; }
    }
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
}
//helpers 

