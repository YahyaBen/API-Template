namespace foodiya.app.Identity.Contracts.Accounts;

/// <summary>Identifies an account operation awaiting email confirmation.</summary>
public sealed record AccountChallengeResponse(Guid ChallengeId, DateTimeOffset ExpiresAtUtc);

/// <summary>Generic recovery acknowledgment independent of account existence.</summary>
public sealed record PasswordRecoveryResponse(string Message);
