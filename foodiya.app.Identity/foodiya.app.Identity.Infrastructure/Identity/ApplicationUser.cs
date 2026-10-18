using Microsoft.AspNetCore.Identity;
using foodiya.app.Identity.Domain.Enums;

namespace foodiya.app.Identity.Infrastructure.Identity;

/// <summary>Identity persistence model shared by all generated services.</summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string PreferredLanguage { get; set; } = "en";
    public AccountStatus Status { get; set; } = AccountStatus.Pending;
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; }
    public DateTimeOffset? DeletedAtUtc { get; set; }
}
