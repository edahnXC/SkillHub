using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SkillHub.SkillHub.Domain.Entities.Identity;

namespace SkillHub.SkillHub.Persistence.Configurations.Identity;

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
               .HasMaxLength(50)
               .IsRequired();

        builder.HasIndex(x => x.Name)
               .IsUnique();

        builder.Property(x => x.Description)
               .HasMaxLength(255);
    }
}