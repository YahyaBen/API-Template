namespace foodiya.app.Identity.Application.Exceptions;

public sealed class IdentityThrottleException(
    string code,
    string message,
    DateTimeOffset retryAfterUtc)
    : IdentityOperationException(code, message)
{
    public DateTimeOffset RetryAfterUtc { get; } = retryAfterUtc;
}
