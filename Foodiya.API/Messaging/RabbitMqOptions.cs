namespace foodiya.app.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";
    public string HostName { get; init; } = "localhost";
    public int Port { get; init; } = 5672;
    public string VirtualHost { get; init; } = "foodiya";
    public string UserName { get; init; } = "foodiya";
    public string Password { get; init; } = string.Empty;
    public string Exchange { get; init; } = "foodiya.identity.events";
    public bool UseTls { get; init; }
    public string? TlsServerName { get; init; }
    public int PublishTimeoutSeconds { get; init; } = 20;
}
