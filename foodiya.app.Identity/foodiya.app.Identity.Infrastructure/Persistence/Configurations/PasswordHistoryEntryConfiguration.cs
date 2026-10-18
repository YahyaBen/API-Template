using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Infrastructure.Identity;

namespace foodiya.app.Identity.Infrastructure.Persistence.Configurations;

internal sealed class PasswordHistoryEntryConfiguration : IEntityTypeConfiguration<PasswordHistoryEntry>
{
    public void Configure(EntityTypeBuilder<PasswordHistoryEntry> builder)
    {
        builder.ToTable("PasswordHistory");
        builder.HasKey(entry => entry.Id);
        builder.Property(entry => entry.PasswordHash).HasMaxLength(512).IsRequired();
        builder.HasIndex(entry => new { entry.UserId, entry.CreatedAtUtc });
        builder.HasOne<ApplicationUser>().WithMany().HasForeignKey(entry => entry.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
