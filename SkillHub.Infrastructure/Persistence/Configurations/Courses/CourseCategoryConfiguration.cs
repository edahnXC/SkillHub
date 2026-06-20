using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SkillHub.Domain.Entities.Courses;

namespace SkillHub.Infrastructure.Persistence.Configurations.Courses;

public class CourseCategoryConfiguration
    : IEntityTypeConfiguration<CourseCategory>
{
    public void Configure(
        EntityTypeBuilder<CourseCategory> builder)
    {
        builder.ToTable("CourseCategories");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);
    }
}