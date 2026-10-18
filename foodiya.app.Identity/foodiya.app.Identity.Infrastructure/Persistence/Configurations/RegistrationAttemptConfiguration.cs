using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using foodiya.app.Identity.Domain.Entities;

namespace foodiya.app.Identity.Infrastructure.Persistence.Configurations;

internal sealed class RegistrationAttemptConfiguration : IEntityTypeConfiguration<RegistrationAttempt>
{
    public void Configure(EntityTypeBuilder<RegistrationAttempt> builder)
    {
        builder.ToTable("RegistrationAttempts");
        builder.HasKey(attempt => attempt.Id);
        builder.Property(attempt => attempt.NormalizedEmail).HasMaxLength(256).IsRequired();
        builder.Property(attempt => attempt.IpAddress).HasMaxLength(64).IsRequired();
        builder.Property(attempt => attempt.RejectionReason).HasMaxLength(200);
        builder.Property(attempt => attempt.UserAgent).HasMaxLength(512);
        builder.HasIndex(attempt => new { attempt.NormalizedEmail, attempt.AttemptedAtUtc });
        builder.HasIndex(attempt => new { attempt.IpAddress, attempt.AttemptedAtUtc });
    }
}
