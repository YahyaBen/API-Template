using RabbitMQ.Client;

namespace foodiya.app.Messaging;

public static class RabbitMqConnectionFactory
{
    public static ConnectionFactory Create(RabbitMqOptions options, string connectionName) => new()
    {
        HostName = options.HostName,
        Port = options.Port,
        VirtualHost = options.VirtualHost,
        UserName = options.UserName,
        Password = options.Password,
        ClientProvidedName = connectionName,
        AutomaticRecoveryEnabled = true,
        TopologyRecoveryEnabled = true,
        NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
        RequestedConnectionTimeout = TimeSpan.FromSeconds(options.PublishTimeoutSeconds),
        ContinuationTimeout = TimeSpan.FromSeconds(options.PublishTimeoutSeconds),
        RequestedHeartbeat = TimeSpan.FromSeconds(30),
        ConsumerDispatchConcurrency = 1,
        Ssl = new SslOption
        {
            Enabled = options.UseTls,
            ServerName = options.TlsServerName ?? options.HostName
        }
    };
}
