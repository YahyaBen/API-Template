using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using foodiya.app.Identity.Domain.Constants;
using foodiya.app.Identity.Domain.Entities;
using foodiya.app.Identity.Infrastructure.Identity;
using foodiya.app.Identity.Infrastructure.Persistence.Configurations;

namespace foodiya.app.Identity.Infrastructure.Persistence;

public sealed class IdentityDataContext(DbContextOptions<IdentityDataContext> options)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<OtpChallenge> OtpChallenges => Set<OtpChallenge>();
    public DbSet<PasswordHistoryEntry> PasswordHistory => Set<PasswordHistoryEntry>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<UserDevice> UserDevices => Set<UserDevice>();
    public DbSet<UserSession> UserSessions => Set<UserSession>();
    public DbSet<RegistrationAttempt> RegistrationAttempts => Set<RegistrationAttempt>();
    public DbSet<OutboxMessage> OutboxMessages => Set<OutboxMessage>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        ConfigureIdentityTables(builder);
        builder.ApplyConfigurationsFromAssembly(typeof(IdentityDataContext).Assembly);
        SeedRoles(builder);
    }

    private static void ConfigureIdentityTables(ModelBuilder builder)
    {
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(user => user.FirstName).HasMaxLength(100).IsRequired();
            entity.Property(user => user.LastName).HasMaxLength(100).IsRequired();
            entity.Property(user => user.PreferredLanguage).HasMaxLength(10).HasDefaultValue("en");
            entity.Property(user => user.Status).HasConversion<int>();
            entity.HasIndex(user => user.Status);
        });

        builder.Entity<IdentityRole<Guid>>().ToTable("Roles");
        builder.Entity<IdentityUserRole<Guid>>().ToTable("UserRoles");
        builder.Entity<IdentityUserClaim<Guid>>().ToTable("UserClaims");
        builder.Entity<IdentityUserLogin<Guid>>().ToTable("UserLogins");
        builder.Entity<IdentityRoleClaim<Guid>>().ToTable("RoleClaims");
        builder.Entity<IdentityUserToken<Guid>>().ToTable("UserTokens");
    }

    private static void SeedRoles(ModelBuilder builder)
    {
        builder.Entity<IdentityRole<Guid>>().HasData(
            CreateRole("11111111-1111-1111-1111-111111111111", RoleNames.SuperAdmin),
            CreateRole("22222222-2222-2222-2222-222222222222", RoleNames.Admin),
            CreateRole("33333333-3333-3333-3333-333333333333", RoleNames.Chef),
            CreateRole("44444444-4444-4444-4444-444444444444", RoleNames.User));
    }

    private static IdentityRole<Guid> CreateRole(string id, string name) => new()
    {
        Id = Guid.Parse(id),
        Name = name,
        NormalizedName = name,
        ConcurrencyStamp = id
    };
}
