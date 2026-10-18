using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace foodiya.app.Messaging;

public static class DependencyInjection
{
    public static IServiceCollection AddRabbitMqMessaging(this IServiceCollection services,
        IConfiguration configuration, bool enabled = true)
    {
        services.AddOptions<RabbitMqOptions>()
            .Bind(configuration.GetSection(RabbitMqOptions.SectionName))
            .Validate(options => !enabled || (!string.IsNullOrWhiteSpace(options.HostName)
                && !string.IsNullOrWhiteSpace(options.VirtualHost)
                && !string.IsNullOrWhiteSpace(options.UserName)
                && !string.IsNullOrWhiteSpace(options.Password)
                && !string.IsNullOrWhiteSpace(options.Exchange)),
                "RabbitMq host, virtual host, user name, password and exchange are required when enabled.")
            .Validate(options => options.Port is >= 1 and <= 65535, "RabbitMq:Port is invalid.")
            .Validate(options => options.PublishTimeoutSeconds is >= 1 and <= 60,
                "RabbitMq:PublishTimeoutSeconds must be between 1 and 60.")
            .ValidateOnStart();
        services.AddSingleton<RabbitMqPublisher>();
        return services;
    }
}
