namespace foodiya.app.Messaging;

public static class IdentityEventRouting
{
    public static string GetRoutingKey(string eventType) => eventType switch
    {
        "UserRegistered" => "identity.user.registered.v1",
        "UserRolesChanged" => "identity.user.roles.changed.v1",
        "UserDeleted" => "identity.user.deleted.v1",
        _ when !string.IsNullOrWhiteSpace(eventType) && eventType.All(char.IsLetterOrDigit)
            => $"identity.{eventType.ToLowerInvariant()}.v1",
        _ => throw new ArgumentException("An event type must contain only letters and digits.", nameof(eventType))
    };
}
