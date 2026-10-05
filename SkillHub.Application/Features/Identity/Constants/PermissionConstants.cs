namespace SkillHub.Application.Features.Identity.Constants;

public static class PermissionConstants
{
    public const string PolicyPrefix = "Permission:";

    // Navigation & UI Tabs (Dynamic Frontend Visibility)
    public const string TabsUsersView              = "Tabs.Users.View";
    public const string TabsCoursesView            = "Tabs.Courses.View";
    public const string TabsCreateUserView         = "Tabs.CreateUser.View";
    public const string TabsRolesAndPermissionsView = "Tabs.RolesAndPermissions.View";
    public const string TabsAnalyticsView          = "Tabs.Analytics.View";
    public const string TabsCategoriesView         = "Tabs.Categories.View";

    // Course (Instructor & Student)
    public const string CoursesCreate       = "Courses.Create";
    public const string CoursesRead         = "Courses.Read";
    public const string CoursesUpdate       = "Courses.Update";
    public const string CoursesDelete       = "Courses.Delete";

    // Course Admin
    public const string CoursesForcePublish = "Courses.ForcePublish";
    public const string CoursesDeleteAny    = "Courses.DeleteAny";
    public const string CoursesArchive      = "Courses.Archive";
    public const string CoursesApprove      = "Courses.Approve";

    // User Management
    public const string UsersView          = "Users.View";
    public const string UsersCreate        = "Users.Create";
    public const string UsersSuspend       = "Users.Suspend";
    public const string UsersDelete        = "Users.Delete";
    public const string UsersResetPassword = "Users.ResetPassword";

    // Category & Tag Management
    public const string CategoriesManage   = "Categories.Manage";
    public const string TagsManage         = "Tags.Manage";

    // Role & Permission Management
    public const string RolesManage        = "Roles.Manage";
    public const string PermissionsManage  = "Permissions.Manage";
    public const string AdminsManage       = "Admins.Manage";

    // Instructor Management
    public const string InstructorsView    = "Instructors.View";
    public const string InstructorsApprove = "Instructors.Approve";

    // Assessments & Quizzes
    public const string QuizzesManage      = "Quizzes.Manage";
    public const string QuizzesTake        = "Quizzes.Take";

    // Reviews
    public const string ReviewsWrite       = "Reviews.Write";
    public const string ReviewsModerate    = "Reviews.Moderate";

    // Support & Helpdesk
    public const string TabsSupportView    = "Tabs.Support.View";
    public const string SupportViewAll     = "Support.ViewAll";
    public const string SupportReply       = "Support.Reply";
    public const string SupportAssign      = "Support.Assign";
    public const string SupportStatusChange = "Support.StatusChange";
}
