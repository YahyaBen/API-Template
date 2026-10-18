namespace foodiya.app.Identity.Domain.Constants;

/// <summary>Canonical role names emitted in access tokens.</summary>
public static class RoleNames
{
    public const string SuperAdmin = "SUPER_ADMIN";
    public const string Admin = "ADMIN";
    public const string Chef = "CHEF";
    public const string User = "USER";

    public static IReadOnlyCollection<string> All { get; } = Array.AsReadOnly(
        [SuperAdmin, Admin, Chef, User]);
}
