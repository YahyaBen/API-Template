using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using foodiya.app.Identity.Domain.Entities;

namespace foodiya.app.Identity.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");
        builder.HasKey(message => message.Id);
        builder.Property(message => message.LeaseId).IsConcurrencyToken();
        builder.Property(message => message.Type).HasMaxLength(500).IsRequired();
        builder.Property(message => message.Payload).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(message => message.Error).HasColumnType("nvarchar(max)");
        builder.HasIndex(message => new { message.ProcessedAtUtc, message.OccurredAtUtc });
        builder.HasIndex(message => new { message.ProcessedAtUtc, message.DeadLetteredAtUtc, message.NextAttemptAtUtc });
    }
}
