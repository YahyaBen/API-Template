using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Options;

namespace ApiTemplate.Hosting;

internal sealed class ApiLimitSettings
{
    public int PermitLimit { get; set; } = 120;
    public int WindowSeconds { get; set; } = 60;
}

internal static class ApiRequestLimits
{
    public static IServiceCollection AddApiRequestLimits(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ApiLimitSettings>().Bind(configuration.GetSection("ApiLimits"))
            .Validate(settings => settings.PermitLimit is >= 1 and <= 10000 && settings.WindowSeconds is >= 1 and <= 3600,
                "ApiLimits requires PermitLimit between 1 and 10000 and WindowSeconds between 1 and 3600.").ValidateOnStart();
        services.AddOptions<Microsoft.AspNetCore.RateLimiting.RateLimiterOptions>().Configure<IOptions<ApiLimitSettings>>((options, settings) =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            options.AddPolicy("api", context => RateLimitPartition.GetFixedWindowLimiter(
                context.User.FindFirst(JwtRegisteredClaimNames.Sub)?.Value
                    ?? context.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions
                {
                    PermitLimit = settings.Value.PermitLimit,
                    Window = TimeSpan.FromSeconds(settings.Value.WindowSeconds),
                    QueueLimit = 0,
                    AutoReplenishment = true
                }));
            options.OnRejected = async (context, _) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retry))
                    context.HttpContext.Response.Headers.RetryAfter = Math.Max(1, (int)Math.Ceiling(retry.TotalSeconds)).ToString(CultureInfo.InvariantCulture);
                var problems = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
                await problems.TryWriteAsync(new ProblemDetailsContext
                {
                    HttpContext = context.HttpContext,
                    ProblemDetails = new ProblemDetails { Status = 429, Title = "Too many requests", Extensions = { ["code"] = "RATE_LIMIT_EXCEEDED" } }
                });
            };
        });
        services.AddRateLimiter();
        services.Configure<KestrelServerOptions>(options => options.Limits.MaxRequestBodySize = 1_048_576);
        return services;
    }
}

internal sealed class PaginationValidationFilter : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        var errors = new Dictionary<string, string[]>();
        if (context.ActionArguments.TryGetValue("page", out var page) && page is int number && number is < 1 or > 10000)
            errors["page"] = ["Page must be between 1 and 10000."];
        if (context.ActionArguments.TryGetValue("pageSize", out var size) && size is int count && count is < 1 or > 100)
            errors["pageSize"] = ["Page size must be between 1 and 100."];
        if (errors.Count > 0)
            context.Result = new BadRequestObjectResult(new ValidationProblemDetails(errors) { Status = 400 });
    }

    public void OnActionExecuted(ActionExecutedContext context) { }
}
