namespace Foodiya.Domain.Models;

public partial class AppUser
{
    public virtual ICollection<AppUserExternalLogin> ExternalLogins { get; set; } = new List<AppUserExternalLogin>();
}
