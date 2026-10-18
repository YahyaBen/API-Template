namespace foodiya.app.Identity.Domain.Enums;

/// <summary>Describes the operation authorized by a one-time password.</summary>
public enum OtpPurpose
{
    Registration = 0,
    DeviceLogin = 1,
    PasswordReset = 2,
    AccountDeletion = 3,
    EmailChange = 4
}
