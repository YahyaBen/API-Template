using System.Threading.RateLimiting;
using Microsoft.Extensions.Options;

namespace foodiya.app.Identity.Api.Middleware;

internal sealed class IngressRateLimitOptions
{
    public int PermitLimit { get; set; } = 300;
}

internal sealed class IngressRateLimiter(IOptions<IngressRateLimitOptions> options) : IDisposable
{
    private readonly PartitionedRateLimiter<HttpContext> _limiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = options.Value.PermitLimit, Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0, AutoReplenishment = true
            }));

    public ValueTask<RateLimitLease> AcquireAsync(HttpContext context) =>
        _limiter.AcquireAsync(context, 1, context.RequestAborted);

    public void Dispose() => _limiter.Dispose();
}

internal sealed class IngressRateLimitMiddleware(RequestDelegate next, IngressRateLimiter limiter)
{
    public async Task InvokeAsync(HttpContext context)
    {
        using var lease = await limiter.AcquireAsync(context);
        if (lease.IsAcquired) await next(context);
        else await IdentityRateLimitingExtensions.WriteRejectionAsync(context, lease);
    }
}
