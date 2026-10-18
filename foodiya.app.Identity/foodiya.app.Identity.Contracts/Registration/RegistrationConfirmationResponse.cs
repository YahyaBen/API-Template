namespace foodiya.app.Identity.Contracts.Registration;

/// <summary>Confirmed account identity.</summary>
public sealed record RegistrationConfirmationResponse(
    Guid UserId,
    string Email,
    bool EmailConfirmed,
    string AccountStatus);
