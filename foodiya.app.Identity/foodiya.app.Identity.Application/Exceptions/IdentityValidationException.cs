namespace foodiya.app.Identity.Application.Exceptions;

public sealed class IdentityValidationException(string code, string message)
    : IdentityOperationException(code, message);
