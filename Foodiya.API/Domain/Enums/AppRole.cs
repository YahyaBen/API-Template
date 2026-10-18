namespace Foodiya.Domain.Enums;

/// <summary>
/// Application roles — mirrors the values stored in AppUser.Role (VARCHAR).
/// Hierarchy: SuperAdmin > Admin > Chef > User.
/// </summary>
public enum AppRole
{
    User = 0,
    Chef = 1,
    Admin = 2,
    SuperAdmin = 3
}
