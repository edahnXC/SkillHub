using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SkillHub.Domain.Entities.Courses;

namespace SkillHub.Infrastructure.Persistence.Configurations.Courses;

public class CourseTagMappingConfiguration
    : IEntityTypeConfiguration<CourseTagMapping>
{
    public void Configure(
        EntityTypeBuilder<CourseTagMapping> builder)
    {
        builder.ToTable("CourseTagMappings");

        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.Course)
            .WithMany(x => x.CourseTagMappings)
            .HasForeignKey(x => x.CourseId);

        builder.HasOne(x => x.Tag)
            .WithMany(x => x.CourseTagMappings)
            .HasForeignKey(x => x.TagId);
    }
}