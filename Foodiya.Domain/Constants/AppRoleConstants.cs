using Foodiya.Domain.Enums;

namespace Foodiya.Domain.Constants;

/// <summary>
/// Application role constants — stored in AppUser.Role column.
/// Hierarchy: SuperAdmin > Admin > Chef > User.
/// </summary>
public static class AppRoleConstants
{
    public const string SuperAdmin = "SUPER_ADMIN";
    public const string Admin = "ADMIN";
    public const string Chef = "CHEF";
    public const string User = "USER";

    /// <summary>
    /// Comma-separated list for [Authorize(Roles = ...)] attributes.
    /// </summary>
    public const string SuperAdminOnly = SuperAdmin;
    public const string AdminOrAbove = $"{SuperAdmin},{Admin}";
    public const string ChefOrAbove = $"{SuperAdmin},{Admin},{Chef}";
    public const string All = $"{SuperAdmin},{Admin},{Chef},{User}";
}
