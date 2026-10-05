using Microsoft.EntityFrameworkCore;
using SkillHub.Domain.Entities.Identity;
using SkillHub.Domain.Entities.Courses;
using SkillHub.Domain.Entities.Learning;
using SkillHub.Domain.Entities.Assessments;
using SkillHub.Domain.Entities.Support;

namespace SkillHub.Infrastructure.Persistence.Context;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    // Identity
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();
    public DbSet<Permission> Permissions => Set<Permission>();
    public DbSet<RolePermission> RolePermissions => Set<RolePermission>();
    public DbSet<UserPermission> UserPermissions => Set<UserPermission>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();

    // Courses
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<Enrollment> Enrollments => Set<Enrollment>();
    public DbSet<CourseCategory> CourseCategories => Set<CourseCategory>();
    public DbSet<CourseSection> CourseSections => Set<CourseSection>();
    public DbSet<CourseLesson> CourseLessons => Set<CourseLesson>();
    public DbSet<LessonResource> LessonResources => Set<LessonResource>();
    public DbSet<CourseTag> CourseTags => Set<CourseTag>();
    public DbSet<CourseTagMapping> CourseTagMappings => Set<CourseTagMapping>();

    // Learning & Progress
    public DbSet<LessonCompletion> LessonCompletions => Set<LessonCompletion>();
    public DbSet<CourseReview> CourseReviews => Set<CourseReview>();
    public DbSet<Certificate> Certificates => Set<Certificate>();

    // Assessments & Quizzes
    public DbSet<Quiz> Quizzes => Set<Quiz>();
    public DbSet<QuizQuestion> QuizQuestions => Set<QuizQuestion>();
    public DbSet<QuizOption> QuizOptions => Set<QuizOption>();
    public DbSet<QuizAttempt> QuizAttempts => Set<QuizAttempt>();
    public DbSet<QuizAnswer> QuizAnswers => Set<QuizAnswer>();

    // Support & Helpdesk
    public DbSet<SupportTicket> SupportTickets => Set<SupportTicket>();
    public DbSet<TicketMessage> TicketMessages => Set<TicketMessage>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // SupportTicket configuration
        modelBuilder.Entity<SupportTicket>(b =>
        {
            b.HasIndex(t => t.TicketNumber).IsUnique();
            b.Property(t => t.TicketNumber).HasMaxLength(50).IsRequired();
            b.Property(t => t.Subject).HasMaxLength(250).IsRequired();
            b.Property(t => t.Category).HasMaxLength(100).IsRequired();
            b.Property(t => t.Priority).HasMaxLength(50).IsRequired();
            b.Property(t => t.Status).HasMaxLength(50).IsRequired();

            b.HasOne(t => t.User)
                .WithMany()
                .HasForeignKey(t => t.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasOne(t => t.AssignedAdmin)
                .WithMany()
                .HasForeignKey(t => t.AssignedAdminId)
                .OnDelete(DeleteBehavior.Restrict);

            b.HasMany(t => t.Messages)
                .WithOne(m => m.Ticket)
                .HasForeignKey(m => m.TicketId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // TicketMessage configuration
        modelBuilder.Entity<TicketMessage>(b =>
        {
            b.Property(m => m.MessageText).IsRequired();

            b.HasOne(m => m.SenderUser)
                .WithMany()
                .HasForeignKey(m => m.SenderUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
