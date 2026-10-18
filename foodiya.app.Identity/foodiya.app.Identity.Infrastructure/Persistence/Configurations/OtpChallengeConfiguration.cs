using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Infrastructure.Identity;

namespace foodiya.app.Identity.Infrastructure.Persistence.Configurations;

internal sealed class OtpChallengeConfiguration : IEntityTypeConfiguration<OtpChallenge>
{
    public void Configure(EntityTypeBuilder<OtpChallenge> builder)
    {
        builder.ToTable("OtpChallenges");
        builder.HasKey(challenge => challenge.Id);
        builder.Property(challenge => challenge.NormalizedEmail).HasMaxLength(256).IsRequired();
        builder.Property(challenge => challenge.CodeHash).HasMaxLength(64).IsFixedLength().IsRequired();
        builder.Property(challenge => challenge.DeviceInstallationIdHash).HasMaxLength(64).IsFixedLength();
        builder.Property(challenge => challenge.IpAddress).HasMaxLength(64);
        builder.HasIndex(challenge => new { challenge.UserId, challenge.Purpose, challenge.ExpiresAtUtc });
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(challenge => challenge.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
