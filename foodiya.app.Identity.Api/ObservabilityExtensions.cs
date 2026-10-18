using OpenTelemetry;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using foodiya.app.Identity.Infrastructure.Operations;

namespace foodiya.app.Identity.Api;

internal static class ObservabilityExtensions
{
    public static IServiceCollection AddIdentityObservability(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var telemetry = services.AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(
                serviceName: "foodiya.app.Identity",
                serviceVersion: typeof(Program).Assembly.GetName().Version?.ToString()))
            .WithTracing(tracing => tracing
                .AddSource(IdentityTelemetry.SourceName)
                .AddSource("foodiya.app.Messaging")
                .AddAspNetCoreInstrumentation(options =>
                    options.Filter = context => !context.Request.Path.StartsWithSegments("/health"))
                .AddHttpClientInstrumentation())
            .WithMetrics(metrics => metrics
                .AddMeter(IdentityTelemetry.MeterName)
                .AddAspNetCoreInstrumentation()
                .AddHttpClientInstrumentation());

        if (!string.IsNullOrWhiteSpace(configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            telemetry.UseOtlpExporter();
        }

        return services;
    }
}
