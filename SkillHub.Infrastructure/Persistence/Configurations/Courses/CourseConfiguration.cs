using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SkillHub.Domain.Entities.Courses;

namespace SkillHub.Infrastructure.Persistence.Configurations.Courses;

public class CourseConfiguration
    : IEntityTypeConfiguration<Course>
{
    public void Configure(
        EntityTypeBuilder<Course> builder)
    {
        builder.ToTable("Courses");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Price)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.DiscountPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.ThumbnailUrl)
            .HasMaxLength(500);

        builder.HasOne(x => x.Category)
            .WithMany(x => x.Courses)
            .HasForeignKey(x => x.CategoryId);

        builder.HasOne(x => x.Instructor)
            .WithMany(x => x.Courses)
            .HasForeignKey(x => x.InstructorId);
    }
}