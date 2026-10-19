using System.Net;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Options;

namespace ApiTemplate.Hosting;

internal sealed class ReverseProxySettings
{
    public bool Enabled { get; set; }
    public int ForwardLimit { get; set; } = 1;
    public string[] KnownProxies { get; set; } = [];
    public string[] KnownNetworks { get; set; } = [];
}

internal static class ApiHostingExtensions
{
    public static IServiceCollection AddApiHosting(this IServiceCollection services,
        IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddOptions<ReverseProxySettings>().Bind(configuration.GetSection("ReverseProxy"))
            .Validate(settings => !settings.Enabled || settings.KnownProxies.Length + settings.KnownNetworks.Length > 0,
                "ReverseProxy requires an explicit trusted proxy or network when enabled.")
            .Validate(settings => settings.ForwardLimit is >= 1 and <= 5,
                "ReverseProxy:ForwardLimit must be between 1 and 5.")
            .Validate(settings => settings.KnownProxies.All(address => IPAddress.TryParse(address, out _)),
                "ReverseProxy:KnownProxies must contain valid IP addresses.")
            .Validate(settings => settings.KnownNetworks.All(network => System.Net.IPNetwork.TryParse(network, out _)),
                "ReverseProxy:KnownNetworks must contain valid CIDR networks.")
            .ValidateOnStart();
        services.AddOptions<ForwardedHeadersOptions>().Configure<IOptions<ReverseProxySettings>>((options, settings) =>
        {
            options.ForwardedHeaders = settings.Value.Enabled
                ? ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto : ForwardedHeaders.None;
            if (!settings.Value.Enabled) return;
            options.ForwardLimit = settings.Value.ForwardLimit;
            options.KnownProxies.Clear();
            options.KnownIPNetworks.Clear();
            foreach (var address in settings.Value.KnownProxies) options.KnownProxies.Add(IPAddress.Parse(address));
            foreach (var network in settings.Value.KnownNetworks) options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse(network));
        });

        var origins = (configuration.GetSection("CorsOrigins").Get<string[]>() ?? [])
            .Where(origin => !string.IsNullOrWhiteSpace(origin)).Select(origin => origin.Trim().TrimEnd('/')).ToArray();
        if (origins.Any(origin => !Uri.TryCreate(origin, UriKind.Absolute, out var uri)
                || !(uri.Scheme == Uri.UriSchemeHttps || environment.IsDevelopment() && uri.Scheme == Uri.UriSchemeHttp)
                || uri.AbsolutePath != "/" || uri.Query.Length != 0 || uri.Fragment.Length != 0 || uri.UserInfo.Length != 0))
            throw new InvalidOperationException("CorsOrigins must contain exact HTTPS origins; HTTP is allowed only in Development.");
        services.AddCors(options => options.AddDefaultPolicy(policy =>
            policy.AllowAnyHeader().AllowAnyMethod().WithOrigins(origins)));
        return services;
    }
}
