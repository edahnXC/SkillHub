using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SkillHub.Domain.Entities.Courses;

namespace SkillHub.Infrastructure.Persistence.Configurations.Courses;

public class CourseTagConfiguration
    : IEntityTypeConfiguration<CourseTag>
{
    public void Configure(
        EntityTypeBuilder<CourseTag> builder)
    {
        builder.ToTable("CourseTags");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();
    }
}