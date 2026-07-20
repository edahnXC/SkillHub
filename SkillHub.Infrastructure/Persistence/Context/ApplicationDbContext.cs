using Microsoft.EntityFrameworkCore;

using SkillHub.Domain.Entities.Identity;
using SkillHub.Domain.Entities.Courses;

namespace SkillHub.Infrastructure.Persistence.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Identity

    public DbSet<User> Users
        => Set<User>();

    public DbSet<Role> Roles
        => Set<Role>();

    public DbSet<UserRole> UserRoles
        => Set<UserRole>();

    public DbSet<Permission> Permissions
        => Set<Permission>();

    public DbSet<RolePermission> RolePermissions
        => Set<RolePermission>();

    public DbSet<RefreshToken> RefreshTokens
        => Set<RefreshToken>();

    // Courses

    public DbSet<Course> Courses
        => Set<Course>();

    public DbSet<Enrollment> Enrollments
        => Set<Enrollment>();

    public DbSet<CourseCategory> CourseCategories
        => Set<CourseCategory>();

    public DbSet<CourseSection> CourseSections
        => Set<CourseSection>();

    public DbSet<CourseLesson> CourseLessons
        => Set<CourseLesson>();

    public DbSet<LessonResource> LessonResources
        => Set<LessonResource>();

    public DbSet<CourseTag> CourseTags
        => Set<CourseTag>();

    public DbSet<CourseTagMapping> CourseTagMappings
        => Set<CourseTagMapping>();


    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(ApplicationDbContext).Assembly);
    }
}