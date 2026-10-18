using System.ComponentModel.DataAnnotations;

namespace foodiya.app.Identity.Contracts.Devices;

/// <summary>Account-owned device metadata without installation hashes or push credentials.</summary>
public sealed record DeviceResponse(Guid Id, string? Platform, string? DeviceName,
    DateTimeOffset FirstSeenAtUtc, DateTimeOffset LastSeenAtUtc, DateTimeOffset? VerifiedAtUtc,
    DateTimeOffset? RevokedAtUtc, bool IsCurrent, bool HasPushRegistration);

/// <summary>A page of devices, ordered by last seen time.</summary>
public sealed record DeviceListResponse(IReadOnlyList<DeviceResponse> Items, int Skip, int Take);

/// <summary>Account-owned session metadata.</summary>
public sealed record SessionResponse(Guid Id, Guid DeviceId, DateTimeOffset CreatedAtUtc,
    DateTimeOffset ExpiresAtUtc, DateTimeOffset? RevokedAtUtc, bool IsActive, bool IsCurrent);

/// <summary>A page of sessions, ordered by creation time.</summary>
public sealed record SessionListResponse(IReadOnlyList<SessionResponse> Items, int Skip, int Take);

/// <summary>Confirms email verification of the current device installation.</summary>
public sealed record ConfirmDeviceVerificationRequest
{
    public required Guid ChallengeId { get; init; }
    [Required, RegularExpression("^[0-9]{6}$")]
    public required string Code { get; init; }
}

/// <summary>Registers the current device's Firebase installation for push notifications.</summary>
public sealed record UpdateFirebaseInstallationRequest
{
    [Required, StringLength(128, MinimumLength = 1)]
    public required string FirebaseInstallationId { get; init; }
}
