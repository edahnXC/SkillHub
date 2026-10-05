using Microsoft.EntityFrameworkCore;
using SkillHub.Domain.Entities.Courses;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Seed;

public static class CourseCategorySeeder
{
    public static async Task SeedCategoriesAsync(ApplicationDbContext context)
    {
        var categoryList = new[]
        {
            new { Name = "Web Development", Desc = "Learn to build modern web applications with Angular, React, and .NET Core" },
            new { Name = "Data Science", Desc = "Master data analysis, data visualization, and statistical modeling with Python & R" },
            new { Name = "AI & ML", Desc = "Dive into Artificial Intelligence, Deep Learning, Neural Networks, and Generative AI" },
            new { Name = "Mobile Dev", Desc = "Build iOS and Android cross-platform apps with Flutter and React Native" },
            new { Name = "Design", Desc = "UI/UX Design principles, wireframing, prototyping, and Figma masterclass" },
            new { Name = "Cloud & DevOps", Desc = "Master AWS, Azure, Docker, Kubernetes, and CI/CD automated deployment pipelines" }
        };

        foreach (var item in categoryList)
        {
            var existing = await context.CourseCategories.FirstOrDefaultAsync(c => c.Name == item.Name);
            if (existing == null)
            {
                await context.CourseCategories.AddAsync(new CourseCategory
                {
                    Name = item.Name,
                    Description = item.Desc,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        await context.SaveChangesAsync();
    }
}
