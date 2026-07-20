using Microsoft.EntityFrameworkCore;

using SkillHub.Application.Features.Identity.Constants;
using SkillHub.Domain.Entities.Identity;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Seed;

public static class PermissionSeeder
{
    public static async Task SeedPermissionsAsync(
        ApplicationDbContext context)
    {
        if (await context.Permissions.AnyAsync())
        {
            return;
        }

        // ── Create all permissions ───────────────────
        var permissions = new List<Permission>
        {
            // Course (Instructor)
            new() { Name = PermissionConstants.CoursesCreate,       Description = "Create new courses" },
            new() { Name = PermissionConstants.CoursesRead,         Description = "View courses" },
            new() { Name = PermissionConstants.CoursesUpdate,       Description = "Update own courses" },
            new() { Name = PermissionConstants.CoursesDelete,       Description = "Delete own courses" },

            // Course Admin (SuperAdmin)
            new() { Name = PermissionConstants.CoursesForcePublish, Description = "Force publish/unpublish any course" },
            new() { Name = PermissionConstants.CoursesDeleteAny,    Description = "Delete any course on the platform" },
            new() { Name = PermissionConstants.CoursesArchive,      Description = "Archive courses" },

            // User Management
            new() { Name = PermissionConstants.UsersView,           Description = "View all user profiles" },
            new() { Name = PermissionConstants.UsersSuspend,        Description = "Suspend/unsuspend user accounts" },
            new() { Name = PermissionConstants.UsersDelete,         Description = "Permanently delete user accounts" },
            new() { Name = PermissionConstants.UsersResetPassword,  Description = "Reset any user's password" },

            // Category & Tag Management
            new() { Name = PermissionConstants.CategoriesManage,    Description = "Create/edit/delete course categories" },
            new() { Name = PermissionConstants.TagsManage,          Description = "Create/edit/delete course tags" },

            // Role & Admin Management
            new() { Name = PermissionConstants.RolesManage,         Description = "Manage roles and assign permissions" },
            new() { Name = PermissionConstants.AdminsManage,        Description = "Create/remove admin accounts" },

            // Instructor Management
            new() { Name = PermissionConstants.InstructorsView,     Description = "View instructor list and details" },
            new() { Name = PermissionConstants.InstructorsApprove,  Description = "Approve/reject instructor applications" }
        };

        await context.Permissions.AddRangeAsync(permissions);
        await context.SaveChangesAsync();

        // ── Fetch roles by name ──────────────────────
        var superAdminRole  = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        var subAdminRole    = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SubAdmin");
        var instructorRole  = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Instructor");
        var studentRole     = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Student");

        // Build lookup: permission name → Id
        var permLookup = await context.Permissions
            .ToDictionaryAsync(p => p.Name, p => p.Id);

        var rolePermissions = new List<RolePermission>();

        // ── SuperAdmin → ALL permissions ─────────────
        if (superAdminRole != null)
        {
            foreach (var permId in permLookup.Values)
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleId = superAdminRole.Id,
                    PermissionId = permId
                });
            }
        }

        // ── SubAdmin → operational subset ────────────
        // Cannot: delete users permanently, change roles, change pricing, delete courses permanently
        if (subAdminRole != null)
        {
            var subAdminPerms = new[]
            {
                PermissionConstants.CoursesRead,
                PermissionConstants.CoursesArchive,
                PermissionConstants.UsersView,
                PermissionConstants.UsersSuspend,
                PermissionConstants.InstructorsView,
                PermissionConstants.InstructorsApprove
            };

            foreach (var permName in subAdminPerms)
            {
                if (permLookup.TryGetValue(permName, out var permId))
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = subAdminRole.Id,
                        PermissionId = permId
                    });
                }
            }
        }

        // ── Instructor → course CRUD ─────────────────
        if (instructorRole != null)
        {
            var instructorPerms = new[]
            {
                PermissionConstants.CoursesCreate,
                PermissionConstants.CoursesRead,
                PermissionConstants.CoursesUpdate,
                PermissionConstants.CoursesDelete
            };

            foreach (var permName in instructorPerms)
            {
                if (permLookup.TryGetValue(permName, out var permId))
                {
                    rolePermissions.Add(new RolePermission
                    {
                        RoleId = instructorRole.Id,
                        PermissionId = permId
                    });
                }
            }
        }

        // ── Student → read-only ──────────────────────
        if (studentRole != null)
        {
            if (permLookup.TryGetValue(PermissionConstants.CoursesRead, out var readPermId))
            {
                rolePermissions.Add(new RolePermission
                {
                    RoleId = studentRole.Id,
                    PermissionId = readPermId
                });
            }
        }

        await context.RolePermissions.AddRangeAsync(rolePermissions);
        await context.SaveChangesAsync();
    }
}
