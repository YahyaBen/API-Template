using System.Globalization;
using System.Security.Claims;
using System.Threading.RateLimiting;
using foodiya.app.Identity.Api.Middleware;
using Microsoft.AspNetCore.RateLimiting;

namespace foodiya.app.Identity.Api;

internal static class IdentityRateLimitingExtensions
{
    public static IServiceCollection AddIdentityRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<IngressRateLimitOptions>().Bind(configuration.GetSection("RateLimits:Ingress"))
            .Validate(options => options.PermitLimit is >= 1 and <= 10000, "RateLimits:Ingress:PermitLimit must be between 1 and 10000.")
            .ValidateOnStart();
        services.AddSingleton<IngressRateLimiter>();
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                RateLimitPartition.GetFixedWindowLimiter(GetKey(context), _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 120, Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true
                }));
            options.AddPolicy("identity-sensitive", context =>
                RateLimitPartition.GetFixedWindowLimiter(GetKey(context), _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 10, Window = TimeSpan.FromMinutes(1), QueueLimit = 0, AutoReplenishment = true
                }));
            options.OnRejected = (context, _) => WriteRejectionAsync(context.HttpContext, context.Lease);
        });
        return services;
    }

    public static async ValueTask WriteRejectionAsync(HttpContext context, RateLimitLease lease)
    {
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        if (lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            context.Response.Headers.RetryAfter = Math.Max(1, (int)Math.Ceiling(retryAfter.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
        var problems = context.RequestServices.GetRequiredService<IProblemDetailsService>();
        await problems.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = context,
            ProblemDetails = new Microsoft.AspNetCore.Mvc.ProblemDetails
            {
                Status = StatusCodes.Status429TooManyRequests,
                Extensions = { ["code"] = "RATE_LIMIT_EXCEEDED", ["traceId"] = context.TraceIdentifier }
            }
        });
    }

    private static string GetKey(HttpContext context) => context.User.FindFirstValue(ClaimTypes.NameIdentifier)
        ?? context.User.FindFirstValue("sub") ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
}
