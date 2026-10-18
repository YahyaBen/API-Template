namespace foodiya.app.Identity.Contracts.Registration;

/// <summary>Metadata for an email confirmation challenge.</summary>
public sealed record RegistrationChallengeResponse(
    Guid ChallengeId,
    string MaskedEmail,
    DateTimeOffset ExpiresAtUtc,
    DateTimeOffset ResendAvailableAtUtc);
