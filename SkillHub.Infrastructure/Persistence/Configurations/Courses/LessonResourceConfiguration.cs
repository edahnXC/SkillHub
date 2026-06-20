using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SkillHub.Domain.Entities.Courses;

namespace SkillHub.Infrastructure.Persistence.Configurations.Courses;

public class LessonResourceConfiguration
    : IEntityTypeConfiguration<LessonResource>
{
    public void Configure(
        EntityTypeBuilder<LessonResource> builder)
    {
        builder.ToTable("LessonResources");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.FileName)
            .HasMaxLength(300)
            .IsRequired();

        builder.Property(x => x.FileUrl)
            .HasMaxLength(1000)
            .IsRequired();

        builder.HasOne(x => x.Lesson)
            .WithMany(x => x.Resources)
            .HasForeignKey(x => x.LessonId);
    }
}