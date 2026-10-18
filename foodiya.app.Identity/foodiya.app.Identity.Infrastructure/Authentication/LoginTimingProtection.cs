using Microsoft.AspNetCore.Identity;
using foodiya.app.Identity.Infrastructure.Identity;

namespace foodiya.app.Identity.Infrastructure.Authentication;

internal sealed class LoginTimingProtection : ILoginTimingProtection
{
    private readonly ApplicationUser _dummyUser = new()
    {
        Id = Guid.NewGuid(),
        FirstName = "Unknown",
        LastName = "User",
        CreatedAtUtc = DateTimeOffset.UnixEpoch
    };
    private readonly PasswordHasher<ApplicationUser> _passwordHasher = new();
    private readonly string _dummyPasswordHash;

    public LoginTimingProtection()
    {
        _dummyPasswordHash = _passwordHasher.HashPassword(
            _dummyUser,
            Guid.NewGuid().ToString("N"));
    }

    public void VerifyDummyPassword(string suppliedPassword) =>
        _ = _passwordHasher.VerifyHashedPassword(
            _dummyUser,
            _dummyPasswordHash,
            suppliedPassword);
}
