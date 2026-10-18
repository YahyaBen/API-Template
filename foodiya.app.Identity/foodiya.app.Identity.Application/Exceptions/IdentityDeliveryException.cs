namespace foodiya.app.Identity.Application.Exceptions;

public sealed class IdentityDeliveryException(string code, string message)
    : IdentityOperationException(code, message);
