using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillHub.Domain.Entities.Identity;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Seed;

public static class RoleSeeder
{
    public static async Task SeedRolesAsync(ApplicationDbContext context)
    {
        var standardRoles = new List<(string Name, string Description)>
        {
            ("SuperAdmin", "Platform owner — unrestricted access to everything"),
            ("SubAdmin", "Operational admin — delegated tasks with restrictions"),
            ("Instructor", "Course Instructor — creates and manages own courses"),
            ("Student", "Platform Student — browses and enrolls in courses")
        };

        foreach (var (name, desc) in standardRoles)
        {
            var role = await context.Roles.FirstOrDefaultAsync(r => r.Name == name);
            if (role == null)
            {
                context.Roles.Add(new Role
                {
                    Name = name,
                    Description = desc,
                    CreatedAt = System.DateTime.UtcNow
                });
            }
        }
        await context.SaveChangesAsync();

        // Clean up any accidental or unwanted custom roles (such as "huh")
        var standardNames = standardRoles.Select(r => r.Name).ToHashSet();
        var extraRoles = await context.Roles
            .Include(r => r.RolePermissions)
            .Include(r => r.UserRoles)
            .Where(r => !standardNames.Contains(r.Name))
            .ToListAsync();

        if (extraRoles.Any())
        {
            context.Roles.RemoveRange(extraRoles);
            await context.SaveChangesAsync();
        }
    }
}