using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using foodiya.app.Identity.Domain.Entities;

namespace foodiya.app.Identity.Integration.Tests.TestSupport;

/// <summary>
/// Keeps the production SQL Server model intact while making its explicitly typed
/// outbox payload columns valid in the relational SQLite test harness.
/// </summary>
internal sealed class SqliteTestModelCustomizer(ModelCustomizerDependencies dependencies)
    : ModelCustomizer(dependencies)
{
    public override void Customize(ModelBuilder modelBuilder, DbContext context)
    {
        base.Customize(modelBuilder, context);
        modelBuilder.Entity<OutboxMessage>().Property(message => message.Payload)
            .HasColumnType("TEXT");
        modelBuilder.Entity<OutboxMessage>().Property(message => message.Error)
            .HasColumnType("TEXT");
        modelBuilder.Entity<OutboxMessage>().Property(message => message.OccurredAtUtc)
            .HasConversion(value => value.UtcTicks, value => new DateTimeOffset(value, TimeSpan.Zero));
        modelBuilder.Entity<OutboxMessage>().Property(message => message.NextAttemptAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.UtcTicks : (long?)null,
                value => value.HasValue ? new DateTimeOffset(value.Value, TimeSpan.Zero) : null);
        modelBuilder.Entity<OutboxMessage>().Property(message => message.LeaseExpiresAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.UtcTicks : (long?)null,
                value => value.HasValue ? new DateTimeOffset(value.Value, TimeSpan.Zero) : null);
        modelBuilder.Entity<OutboxMessage>().Property(message => message.ProcessedAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.UtcTicks : (long?)null,
                value => value.HasValue ? new DateTimeOffset(value.Value, TimeSpan.Zero) : null);

        // SQLite cannot translate ordering comparisons over DateTimeOffset. The
        // security tests store UTC ticks so the atomic refresh-token predicate
        // exercises the same chronological comparison used by SQL Server.
        modelBuilder.Entity<RefreshToken>().Property(token => token.ExpiresAtUtc)
            .HasConversion(
                value => value.UtcTicks,
                value => new DateTimeOffset(value, TimeSpan.Zero));
        modelBuilder.Entity<OtpChallenge>().Property(otp => otp.CreatedAtUtc)
            .HasConversion(value => value.UtcTicks, value => new DateTimeOffset(value, TimeSpan.Zero));
        modelBuilder.Entity<OtpChallenge>().Property(otp => otp.ExpiresAtUtc)
            .HasConversion(value => value.UtcTicks, value => new DateTimeOffset(value, TimeSpan.Zero));
        modelBuilder.Entity<PasswordHistoryEntry>().Property(entry => entry.CreatedAtUtc)
            .HasConversion(value => value.UtcTicks, value => new DateTimeOffset(value, TimeSpan.Zero));
        modelBuilder.Entity<UserDevice>().Property(device => device.LastSeenAtUtc)
            .HasConversion(value => value.UtcTicks, value => new DateTimeOffset(value, TimeSpan.Zero));
        modelBuilder.Entity<UserSession>().Property(session => session.CreatedAtUtc)
            .HasConversion(value => value.UtcTicks, value => new DateTimeOffset(value, TimeSpan.Zero));
        modelBuilder.Entity<UserSession>().Property(session => session.ExpiresAtUtc)
            .HasConversion(value => value.UtcTicks, value => new DateTimeOffset(value, TimeSpan.Zero));
        modelBuilder.Entity<UserSession>().Property(session => session.RevokedAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.UtcTicks : (long?)null,
                value => value.HasValue ? new DateTimeOffset(value.Value, TimeSpan.Zero) : null);
        modelBuilder.Entity<RegistrationAttempt>().Property(attempt => attempt.AttemptedAtUtc)
            .HasConversion(value => value.UtcTicks, value => new DateTimeOffset(value, TimeSpan.Zero));
        modelBuilder.Entity<foodiya.app.Identity.Infrastructure.Identity.ApplicationUser>().Property(user => user.DeletedAtUtc)
            .HasConversion(
                value => value.HasValue ? value.Value.UtcTicks : (long?)null,
                value => value.HasValue ? new DateTimeOffset(value.Value, TimeSpan.Zero) : null);
    }
}
