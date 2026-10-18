using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Infrastructure.Identity;

namespace foodiya.app.Identity.Infrastructure.Persistence.Configurations;

internal sealed class UserSessionConfiguration : IEntityTypeConfiguration<UserSession>
{
    public void Configure(EntityTypeBuilder<UserSession> builder)
    {
        builder.ToTable("UserSessions");
        builder.HasKey(session => session.Id);
        builder.Property(session => session.IpAddress).HasMaxLength(64);
        builder.Property(session => session.RevocationReason).HasMaxLength(100);
        builder.HasIndex(session => new { session.UserId, session.RevokedAtUtc, session.ExpiresAtUtc });
        builder.HasIndex(session => new { session.UserId, session.Id, session.DeviceId });
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(session => session.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<UserDevice>().WithMany().HasForeignKey(session => session.DeviceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
