using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SkillHub.Domain.Entities.Courses;

namespace SkillHub.Infrastructure.Persistence.Configurations.Courses;

public class CourseLessonConfiguration
    : IEntityTypeConfiguration<CourseLesson>
{
    public void Configure(
        EntityTypeBuilder<CourseLesson> builder)
    {
        builder.ToTable("CourseLessons");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.VideoUrl)
            .HasMaxLength(1000);

        builder.HasOne(x => x.Section)
            .WithMany(x => x.Lessons)
            .HasForeignKey(x => x.SectionId);
    }
}