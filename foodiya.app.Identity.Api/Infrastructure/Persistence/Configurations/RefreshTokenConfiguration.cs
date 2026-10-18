using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Infrastructure.Identity;

namespace foodiya.app.Identity.Infrastructure.Persistence.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(token => token.Id);
        builder.Property(token => token.TokenHash).HasMaxLength(64).IsFixedLength().IsRequired();
        builder.Property(token => token.ReplacedByTokenHash).HasMaxLength(64).IsFixedLength();
        builder.Property(token => token.CreatedByIp).HasMaxLength(64);
        builder.Property(token => token.RevokedByIp).HasMaxLength(64);
        builder.Property(token => token.RevocationReason).HasMaxLength(100);
        builder.HasIndex(token => token.TokenHash).IsUnique();
        builder.HasIndex(token => new { token.UserId, token.RevokedAtUtc, token.ExpiresAtUtc });
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(token => token.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<UserSession>().WithMany().HasForeignKey(token => token.SessionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
