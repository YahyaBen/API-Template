namespace foodiya.app.Identity.Application.Exceptions;

public sealed class IdentityForbiddenException(string code, string message)
    : IdentityOperationException(code, message);
