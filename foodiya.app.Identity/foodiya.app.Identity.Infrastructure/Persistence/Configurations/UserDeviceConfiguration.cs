using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Infrastructure.Identity;

namespace foodiya.app.Identity.Infrastructure.Persistence.Configurations;

internal sealed class UserDeviceConfiguration : IEntityTypeConfiguration<UserDevice>
{
    public void Configure(EntityTypeBuilder<UserDevice> builder)
    {
        builder.ToTable("UserDevices");
        builder.HasKey(device => device.Id);
        builder.Property(device => device.InstallationIdHash).HasMaxLength(64).IsFixedLength().IsRequired();
        builder.Property(device => device.Platform).HasMaxLength(30);
        builder.Property(device => device.DeviceName).HasMaxLength(150);
        builder.Property(device => device.OsVersion).HasMaxLength(50);
        builder.Property(device => device.AppVersion).HasMaxLength(50);
        builder.Property(device => device.FirebaseInstallationId).HasMaxLength(128);
        builder.Property(device => device.RevocationReason).HasMaxLength(100);
        builder.Property(device => device.LastIpAddress).HasMaxLength(64);
        builder.HasIndex(device => new { device.UserId, device.InstallationIdHash }).IsUnique();
        builder.HasIndex(device => new { device.UserId, device.LastSeenAtUtc });
        builder.HasIndex(device => device.FirebaseInstallationId).IsUnique()
            .HasFilter("[FirebaseInstallationId] IS NOT NULL");
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(device => device.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
