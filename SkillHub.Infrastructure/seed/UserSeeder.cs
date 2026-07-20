using Microsoft.EntityFrameworkCore;
using SkillHub.Domain.Entities.Identity;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Seed;

public static class UserSeeder
{
    public static async Task SeedUsersAsync(ApplicationDbContext context)
    {
        var superAdminRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        var subAdminRole   = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SubAdmin");
        var instructorRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Instructor");
        var studentRole    = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Student");

        // ── SuperAdmin ───────────────────────────────
        if (superAdminRole != null)
        {
            var existing = await context.Users.FirstOrDefaultAsync(u => u.Email == "superadmin@skillhub.com");
            if (existing != null)
            {
                existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword("SuperAdmin123!");
            }
            else
            {
                var user = new User
                {
                    FirstName = "Super",
                    LastName = "Admin",
                    Email = "superadmin@skillhub.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("SuperAdmin123!"),
                    IsActive = true,
                    IsEmailVerified = true,
                    CreatedAt = DateTime.UtcNow
                };
                user.UserRoles.Add(new UserRole { RoleId = superAdminRole.Id, User = user });
                await context.Users.AddAsync(user);
            }
        }

        // ── SubAdmin ─────────────────────────────────
        if (subAdminRole != null)
        {
            var existing = await context.Users.FirstOrDefaultAsync(u => u.Email == "subadmin@skillhub.com");
            if (existing != null)
            {
                existing.PasswordHash = BCrypt.Net.BCrypt.HashPassword("SubAdmin123!");
            }
            else
            {
                var user = new User
                {
                    FirstName = "Sub",
                    LastName = "Admin",
                    Email = "subadmin@skillhub.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("SubAdmin123!"),
                    IsActive = true,
                    IsEmailVerified = true,
                    CreatedAt = DateTime.UtcNow
                };
                user.UserRoles.Add(new UserRole { RoleId = subAdminRole.Id, User = user });
                await context.Users.AddAsync(user);
            }
        }

        // ── Instructor ───────────────────────────────
        if (instructorRole != null)
        {
            var existing = await context.Users.FirstOrDefaultAsync(u => u.Email == "instructor@skillhub.com");
            if (existing == null)
            {
                var user = new User
                {
                    FirstName = "Expert",
                    LastName = "Instructor",
                    Email = "instructor@skillhub.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Instructor123!"),
                    IsActive = true,
                    IsEmailVerified = true,
                    CreatedAt = DateTime.UtcNow
                };
                user.UserRoles.Add(new UserRole { RoleId = instructorRole.Id, User = user });
                await context.Users.AddAsync(user);
            }
        }

        // ── Student ──────────────────────────────────
        if (studentRole != null)
        {
            var existing = await context.Users.FirstOrDefaultAsync(u => u.Email == "student@skillhub.com");
            if (existing == null)
            {
                var user = new User
                {
                    FirstName = "Normal",
                    LastName = "Student",
                    Email = "student@skillhub.com",
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("Student123!"),
                    IsActive = true,
                    IsEmailVerified = true,
                    CreatedAt = DateTime.UtcNow
                };
                user.UserRoles.Add(new UserRole { RoleId = studentRole.Id, User = user });
                await context.Users.AddAsync(user);
            }
        }

        await context.SaveChangesAsync();
    }
}
