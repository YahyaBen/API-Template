using System.Diagnostics;
using System.Security.Claims;

namespace foodiya.app.Identity.Api.Middleware;

internal sealed class IdentityAuditMiddleware(RequestDelegate next, ILogger<IdentityAuditMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        var started = Stopwatch.GetTimestamp();
        try
        {
            await next(context);
        }
        finally
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                var subject = context.User.FindFirstValue("sub")
                    ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? "anonymous";
                logger.LogInformation(
                    "Identity audit {Method} {Path} returned {StatusCode} for subject {SubjectId} in {ElapsedMilliseconds:F1} ms; trace {TraceId}.",
                    context.Request.Method,
                    context.Request.Path,
                    context.Response.StatusCode,
                    subject,
                    Stopwatch.GetElapsedTime(started).TotalMilliseconds,
                    context.TraceIdentifier);
            }
        }
    }
}
