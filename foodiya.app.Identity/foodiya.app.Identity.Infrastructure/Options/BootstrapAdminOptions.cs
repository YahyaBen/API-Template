namespace foodiya.app.Identity.Infrastructure.Options;

/// <summary>Controls one-time provisioning of the first super administrator.</summary>
public sealed class BootstrapAdminOptions
{
    public const string SectionName = "BootstrapAdmin";

    public bool Enabled { get; set; }
    public string Email { get; set; } = string.Empty;
    public string UserName { get; set; } = "superadmin";
    public string Password { get; set; } = string.Empty;
    public string FirstName { get; set; } = "System";
    public string LastName { get; set; } = "Administrator";
}
