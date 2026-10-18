namespace foodiya.app.Identity.Application.Exceptions;

public sealed class IdentityConflictException(string code, string message)
    : IdentityOperationException(code, message);
