namespace foodiya.app.Identity.Application.Exceptions;

public sealed class IdentityUnauthorizedException(string code, string message)
    : IdentityOperationException(code, message);
