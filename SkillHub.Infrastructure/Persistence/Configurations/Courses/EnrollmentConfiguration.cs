using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillHub.Domain.Entities.Courses;

namespace SkillHub.Infrastructure.Persistence.Configurations.Courses;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.ToTable("Enrollments");

        builder.HasKey(e => e.Id);

        // Ensure unique index so a student cannot enroll in the same course twice
        builder.HasIndex(e => new { e.StudentId, e.CourseId })
            .IsUnique();

        builder.HasOne(e => e.Student)
            .WithMany()
            .HasForeignKey(e => e.StudentId)
            .OnDelete(DeleteBehavior.Restrict); // Prevent multiple cascade paths in SQL Server

        builder.HasOne(e => e.Course)
            .WithMany()
            .HasForeignKey(e => e.CourseId)
            .OnDelete(DeleteBehavior.Cascade); // Cascade delete if course is deleted
    }
}
