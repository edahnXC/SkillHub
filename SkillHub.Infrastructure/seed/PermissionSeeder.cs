using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SkillHub.Application.Features.Identity.Constants;
using SkillHub.Domain.Entities.Identity;
using SkillHub.Infrastructure.Persistence.Context;

namespace SkillHub.Infrastructure.Seed;

public static class PermissionSeeder
{
    public static async Task SeedPermissionsAsync(ApplicationDbContext context)
    {
        // Define all platform permissions
        var allPermissions = new List<(string Name, string Description)>
        {
            // UI Navigation Tabs
            (PermissionConstants.TabsUsersView, "Access Users tab in Admin panel"),
            (PermissionConstants.TabsCoursesView, "Access Courses tab in Admin panel"),
            (PermissionConstants.TabsCreateUserView, "Access Create User tab in Admin panel"),
            (PermissionConstants.TabsRolesAndPermissionsView, "Access Roles & Dynamic Permission Studio tab"),
            (PermissionConstants.TabsAnalyticsView, "Access Platform Analytics & Reporting tab"),
            (PermissionConstants.TabsCategoriesView, "Access Category Management tab"),

            // Course Management
            (PermissionConstants.CoursesCreate, "Create new courses"),
            (PermissionConstants.CoursesRead, "View published courses"),
            (PermissionConstants.CoursesUpdate, "Update own courses"),
            (PermissionConstants.CoursesDelete, "Delete own courses"),
            (PermissionConstants.CoursesForcePublish, "Force publish or unpublish any course"),
            (PermissionConstants.CoursesDeleteAny, "Delete any course across platform"),
            (PermissionConstants.CoursesArchive, "Archive courses"),
            (PermissionConstants.CoursesApprove, "Approve submitted courses"),

            // User Management
            (PermissionConstants.UsersView, "View user lists and detailed profiles"),
            (PermissionConstants.UsersCreate, "Create administrative and standard accounts"),
            (PermissionConstants.UsersSuspend, "Suspend or unsuspend user accounts"),
            (PermissionConstants.UsersDelete, "Permanently delete user accounts"),
            (PermissionConstants.UsersResetPassword, "Reset any user password"),

            // Categories & Tags
            (PermissionConstants.CategoriesManage, "Create, edit, and delete course categories"),
            (PermissionConstants.TagsManage, "Create, edit, and delete course tags"),

            // Roles & Permissions
            (PermissionConstants.RolesManage, "Manage roles and dynamic permission matrix"),
            (PermissionConstants.PermissionsManage, "Assign and revoke granular permissions"),
            (PermissionConstants.AdminsManage, "Create and manage admin staff"),

            // Instructors
            (PermissionConstants.InstructorsView, "View instructor directory and earnings"),
            (PermissionConstants.InstructorsApprove, "Approve or reject instructor applications"),

            // Quizzes & Assessments
            (PermissionConstants.QuizzesManage, "Create and manage course quizzes"),
            (PermissionConstants.QuizzesTake, "Take course quizzes and receive certificates"),

            // Reviews
            (PermissionConstants.ReviewsWrite, "Write and submit course reviews"),
            (PermissionConstants.ReviewsModerate, "Moderate and delete inappropriate reviews"),

            // Support & Helpdesk
            (PermissionConstants.TabsSupportView, "Access Support & Helpdesk tab in Admin panel"),
            (PermissionConstants.SupportViewAll, "View all platform support tickets"),
            (PermissionConstants.SupportReply, "Reply to tickets and post internal staff notes"),
            (PermissionConstants.SupportAssign, "Assign support tickets to staff members"),
            (PermissionConstants.SupportStatusChange, "Change ticket status (Resolve, Close, In-Progress)")
        };

        foreach (var (name, desc) in allPermissions)
        {
            var existing = await context.Permissions.FirstOrDefaultAsync(p => p.Name == name);
            if (existing == null)
            {
                context.Permissions.Add(new Permission
                {
                    Name = name,
                    Description = desc,
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        await context.SaveChangesAsync();

        var superAdminRole  = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SuperAdmin");
        var subAdminRole    = await context.Roles.FirstOrDefaultAsync(r => r.Name == "SubAdmin");
        var instructorRole  = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Instructor");
        var studentRole     = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Student");

        var permLookup = await context.Permissions.ToDictionaryAsync(p => p.Name, p => p.Id);

        // SuperAdmin -> ALL Permissions EXCEPT ticket mutation/assignment (SuperAdmin is Read-Only Oversight on Support)
        if (superAdminRole != null)
        {
            foreach (var perm in permLookup)
            {
                // SuperAdmin has read-only oversight on support: no assign, status change, or staff reply
                if (perm.Key is PermissionConstants.SupportAssign or PermissionConstants.SupportStatusChange or PermissionConstants.SupportReply)
                {
                    var existingRp = await context.RolePermissions.FirstOrDefaultAsync(rp => rp.RoleId == superAdminRole.Id && rp.PermissionId == perm.Value);
                    if (existingRp != null)
                    {
                        context.RolePermissions.Remove(existingRp);
                    }
                    continue;
                }

                var hasRp = await context.RolePermissions.AnyAsync(rp => rp.RoleId == superAdminRole.Id && rp.PermissionId == perm.Value);
                if (!hasRp)
                {
                    context.RolePermissions.Add(new RolePermission
                    {
                        RoleId = superAdminRole.Id,
                        PermissionId = perm.Value,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        // SubAdmin -> Operations Subset (Users Tab, Courses Tab, Categories, Approvals, Solve Assigned Tickets)
        if (subAdminRole != null)
        {
            // Remove SupportAssign if present (only Product Support assigns tickets)
            if (permLookup.TryGetValue(PermissionConstants.SupportAssign, out var assignPid))
            {
                var assignRp = await context.RolePermissions.FirstOrDefaultAsync(rp => rp.RoleId == subAdminRole.Id && rp.PermissionId == assignPid);
                if (assignRp != null) context.RolePermissions.Remove(assignRp);
            }

            var subAdminPerms = new[]
            {
                PermissionConstants.TabsUsersView,
                PermissionConstants.TabsCoursesView,
                PermissionConstants.TabsCategoriesView,
                PermissionConstants.CoursesRead,
                PermissionConstants.CoursesArchive,
                PermissionConstants.CoursesApprove,
                PermissionConstants.UsersView,
                PermissionConstants.UsersSuspend,
                PermissionConstants.CategoriesManage,
                PermissionConstants.InstructorsView,
                PermissionConstants.InstructorsApprove,
                PermissionConstants.ReviewsModerate,
                PermissionConstants.TabsSupportView,
                PermissionConstants.SupportViewAll,
                PermissionConstants.SupportReply,
                PermissionConstants.SupportStatusChange
            };

            foreach (var pName in subAdminPerms)
            {
                if (permLookup.TryGetValue(pName, out var pid))
                {
                    var hasRp = await context.RolePermissions.AnyAsync(rp => rp.RoleId == subAdminRole.Id && rp.PermissionId == pid);
                    if (!hasRp)
                    {
                        context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = subAdminRole.Id,
                            PermissionId = pid,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
        }

        // ProductSupport -> Helpdesk Command (ViewAll, Assign to Admins, Reply, StatusChange)
        var productSupportRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "ProductSupport");
        if (productSupportRole != null)
        {
            var supportPerms = new[]
            {
                PermissionConstants.TabsSupportView,
                PermissionConstants.SupportViewAll,
                PermissionConstants.SupportReply,
                PermissionConstants.SupportAssign,
                PermissionConstants.SupportStatusChange
            };

            foreach (var pName in supportPerms)
            {
                if (permLookup.TryGetValue(pName, out var pid))
                {
                    var hasRp = await context.RolePermissions.AnyAsync(rp => rp.RoleId == productSupportRole.Id && rp.PermissionId == pid);
                    if (!hasRp)
                    {
                        context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = productSupportRole.Id,
                            PermissionId = pid,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
        }

        // Instructor -> Course CRUD, Quizzes
        if (instructorRole != null)
        {
            var instructorPerms = new[]
            {
                PermissionConstants.CoursesCreate,
                PermissionConstants.CoursesRead,
                PermissionConstants.CoursesUpdate,
                PermissionConstants.CoursesDelete,
                PermissionConstants.QuizzesManage
            };

            foreach (var pName in instructorPerms)
            {
                if (permLookup.TryGetValue(pName, out var pid))
                {
                    var hasRp = await context.RolePermissions.AnyAsync(rp => rp.RoleId == instructorRole.Id && rp.PermissionId == pid);
                    if (!hasRp)
                    {
                        context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = instructorRole.Id,
                            PermissionId = pid,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
        }

        // Student -> Course Read, Quizzes Take, Reviews Write
        if (studentRole != null)
        {
            var studentPerms = new[]
            {
                PermissionConstants.CoursesRead,
                PermissionConstants.QuizzesTake,
                PermissionConstants.ReviewsWrite
            };

            foreach (var pName in studentPerms)
            {
                if (permLookup.TryGetValue(pName, out var pid))
                {
                    var hasRp = await context.RolePermissions.AnyAsync(rp => rp.RoleId == studentRole.Id && rp.PermissionId == pid);
                    if (!hasRp)
                    {
                        context.RolePermissions.Add(new RolePermission
                        {
                            RoleId = studentRole.Id,
                            PermissionId = pid,
                            CreatedAt = DateTime.UtcNow
                        });
                    }
                }
            }
        }

        await context.SaveChangesAsync();
    }
}
