using Foodiya.Domain.Interfaces.Core;
using Foodiya.Infrastructure.Extensions.Data;
using Foodiya.Infrastructure.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using foodiya.app.Messaging;
using Foodiya.Infrastructure.Messaging;
using Foodiya.Infrastructure.Identity;

namespace Foodiya.Infrastructure.Extensions
{
    public static partial class ServiceCollectionExtensions
    {
        public static void AddInfrastructure(this IServiceCollection services, IConfiguration configuration, bool isDevelopment = false)
        {
            services.AddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
            services.AddSingleton(TimeProvider.System);
            services.AddScoped<IdentityProfileProvisioner>();
            services.AddRepositories(configuration, isDevelopment);
            services.AddHealthChecks();
            services.AddOptions<IdentityEventsOptions>()
                .Bind(configuration.GetSection(IdentityEventsOptions.SectionName))
                .Validate(options => !options.Enabled || !string.IsNullOrWhiteSpace(options.Queue), "IdentityEvents:Queue is required.")
                .Validate(options => options.MaxAttempts is >= 1 and <= 100, "IdentityEvents:MaxAttempts must be between 1 and 100.")
                .Validate(options => options.RetryDelaySeconds is >= 1 and <= 60, "IdentityEvents:RetryDelaySeconds must be between 1 and 60.")
                .ValidateOnStart();
            if (configuration.GetValue<bool>("IdentityEvents:Enabled"))
            {
                services.AddRabbitMqMessaging(configuration);
                services.AddScoped<IdentityProfileEventHandler>();
                services.AddSingleton<IdentityEventsConsumer>();
                services.AddHostedService(provider => provider.GetRequiredService<IdentityEventsConsumer>());
                services.AddHealthChecks().AddCheck<IdentityConsumerHealthCheck>("identity-events");
            }
            // Add Other Services as "Azure AD", "Mail Jet", "DocuSign" ...
        }
    }
}
