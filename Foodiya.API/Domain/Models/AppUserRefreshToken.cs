using System.ComponentModel.DataAnnotations.Schema;

namespace Foodiya.Domain.Models;

public partial class AppUserRefreshToken
{
    public bool IsExpired(DateTime utcNow) => utcNow >= ExpiresAtUtc;

    [NotMapped]
    public bool IsRevoked => RevokedAtUtc is not null;

    public bool IsActive(DateTime utcNow) => !IsRevoked && !IsExpired(utcNow);
}
