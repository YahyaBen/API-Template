using System.ComponentModel.DataAnnotations;

namespace foodiya.app.Identity.Contracts.Roles;

/// <summary>Complete replacement set for an account's role assignments.</summary>
public sealed record ReplaceUserRolesRequest
{
    /// <summary>One or more canonical roles to assign to the account.</summary>
    [Required, MinLength(1)]
    public required IReadOnlyCollection<string> Roles { get; init; }
}
