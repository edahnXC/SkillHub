using Microsoft.EntityFrameworkCore;

using SkillHub.Domain.Entities.Identity;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Seed;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(
        ApplicationDbContext context)
    {
        if (await context.Roles.AnyAsync())
        {
            return;
        }

        var roles = new List<Role>
        {
            new()
            {
                Name = "SuperAdmin",
                Description = "Platform owner — unrestricted access to everything"
            },

            new()
            {
                Name = "SubAdmin",
                Description = "Operational admin — delegated tasks with restrictions"
            },

            new()
            {
                Name = "Instructor",
                Description = "Course Instructor — creates and manages own courses"
            },

            new()
            {
                Name = "Student",
                Description = "Platform Student — browses and enrolls in courses"
            }
        };

        await context.Roles.AddRangeAsync(roles);

        await context.SaveChangesAsync();
    }
}