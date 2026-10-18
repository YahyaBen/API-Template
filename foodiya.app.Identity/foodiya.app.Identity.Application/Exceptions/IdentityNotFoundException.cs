namespace foodiya.app.Identity.Application.Exceptions;

public sealed class IdentityNotFoundException(string code, string message)
    : IdentityOperationException(code, message);
