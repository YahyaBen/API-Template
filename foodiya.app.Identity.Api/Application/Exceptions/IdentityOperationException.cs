namespace foodiya.app.Identity.Application.Exceptions;

public abstract class IdentityOperationException(string code, string message) : Exception(message)
{
    public string Code { get; } = code;
}
