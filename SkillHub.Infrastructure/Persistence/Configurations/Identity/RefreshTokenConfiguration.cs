using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using SkillHub.Domain.Entities.Identity;

namespace SkillHub.Infrastructure.Persistence.Configurations.Identity;

public class RefreshTokenConfiguration
    : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(
        EntityTypeBuilder<RefreshToken> builder)
    {
        // Map to existing table name (singular, from initial migration)
        builder.ToTable("RefreshToken");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Token)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.CreatedByIp)
            .HasMaxLength(50);

        builder.Property(x => x.ReplacedByToken)
            .HasMaxLength(500);

        // IsActive is a computed property, not mapped to DB
        builder.Ignore(x => x.IsActive);

        builder.HasOne(x => x.User)
            .WithMany(x => x.RefreshTokens)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.Token);
    }
}
