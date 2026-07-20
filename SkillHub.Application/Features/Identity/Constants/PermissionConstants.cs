namespace SkillHub.Application.Features.Identity.Constants;

public static class PermissionConstants
{
    // ── Course Permissions (Instructor) ──────────────
    public const string CoursesCreate = "Courses.Create";
    public const string CoursesRead = "Courses.Read";
    public const string CoursesUpdate = "Courses.Update";
    public const string CoursesDelete = "Courses.Delete";

    // ── Course Admin Permissions (SuperAdmin only) ───
    public const string CoursesForcePublish = "Courses.ForcePublish";
    public const string CoursesDeleteAny = "Courses.DeleteAny";
    public const string CoursesArchive = "Courses.Archive";

    // ── User Management ──────────────────────────────
    public const string UsersView = "Users.View";
    public const string UsersSuspend = "Users.Suspend";
    public const string UsersDelete = "Users.Delete";
    public const string UsersResetPassword = "Users.ResetPassword";

    // ── Category & Tag Management (SuperAdmin) ───────
    public const string CategoriesManage = "Categories.Manage";
    public const string TagsManage = "Tags.Manage";

    // ── Role & Admin Management (SuperAdmin) ─────────
    public const string RolesManage = "Roles.Manage";
    public const string AdminsManage = "Admins.Manage";

    // ── Instructor Management (SubAdmin+) ────────────
    public const string InstructorsView = "Instructors.View";
    public const string InstructorsApprove = "Instructors.Approve";

    // ── Policy prefix for dynamic policy provider ────
    public const string PolicyPrefix = "Permission:";
}
