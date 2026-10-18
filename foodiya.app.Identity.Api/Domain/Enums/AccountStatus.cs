namespace foodiya.app.Identity.Domain.Enums;

/// <summary>Represents the lifecycle state of an identity account.</summary>
public enum AccountStatus
{
    Pending = 0,
    Active = 1,
    Suspended = 2,
    Deleted = 3
}
