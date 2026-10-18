using System.ComponentModel.DataAnnotations;

namespace foodiya.app.Identity.Contracts.Registration;

/// <summary>Data required to create a pending identity account.</summary>
public sealed record RegisterRequest
{
    /// <summary>Unique sign-in name.</summary>
    [Required, StringLength(50, MinimumLength = 3)]
    public required string UserName { get; init; }

    /// <summary>Email address that will receive the confirmation code.</summary>
    [Required, EmailAddress, StringLength(256)]
    public required string Email { get; init; }

    /// <summary>Initial account password.</summary>
    [Required, StringLength(128, MinimumLength = 8)]
    public required string Password { get; init; }

    /// <summary>User's given name.</summary>
    [Required, StringLength(100)]
    public required string FirstName { get; init; }

    /// <summary>User's family name.</summary>
    [Required, StringLength(100)]
    public required string LastName { get; init; }

    /// <summary>Preferred ISO language code.</summary>
    [StringLength(10, MinimumLength = 2)]
    public string PreferredLanguage { get; init; } = "en";
}
