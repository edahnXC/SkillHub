using Microsoft.EntityFrameworkCore;
using SkillHub.Domain.Entities.Courses;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Seed;

public static class CourseCategorySeeder
{
    public static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        if (await context.CourseCategories.AnyAsync())
        {
            return; // Categories already exist
        }

        var categories = new List<CourseCategory>
        {
            new CourseCategory
            {
                Name = "Web Development",
                Description = "Learn to build modern websites and web applications",
                CreatedAt = DateTime.UtcNow
            },
            new CourseCategory
            {
                Name = "Data Science",
                Description = "Master data analysis, machine learning, and AI",
                CreatedAt = DateTime.UtcNow
            },
            new CourseCategory
            {
                Name = "Mobile Development",
                Description = "Build native and cross-platform mobile apps",
                CreatedAt = DateTime.UtcNow
            },
            new CourseCategory
            {
                Name = "Design",
                Description = "UI/UX design, graphic design, and 3D modeling",
                CreatedAt = DateTime.UtcNow
            },
            new CourseCategory
            {
                Name = "Business",
                Description = "Entrepreneurship, management, and marketing skills",
                CreatedAt = DateTime.UtcNow
            }
        };

        await context.CourseCategories.AddRangeAsync(categories);
        await context.SaveChangesAsync();
    }
}
