using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SkillHub.Domain.Entities.Courses;

namespace SkillHub.Infrastructure.Persistence.Configurations.Courses;

public class CourseSectionConfiguration: IEntityTypeConfiguration<CourseSection>
{
    public void Configure(
        EntityTypeBuilder<CourseSection> builder)
    {
        builder.ToTable("CourseSections");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .HasMaxLength(200).IsRequired();
        builder.HasOne(x => x.Course)
            .WithMany(x => x.Sections)
            .HasForeignKey(x => x.CourseId);
    }
}