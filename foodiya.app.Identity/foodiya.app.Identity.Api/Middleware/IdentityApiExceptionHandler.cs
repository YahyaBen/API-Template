using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;
using foodiya.app.Identity.Application.Exceptions;

namespace foodiya.app.Identity.Api.Middleware;

internal sealed class IdentityApiExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<IdentityApiExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var identityException = exception as IdentityOperationException;
        var statusCode = identityException is not null
            ? GetIdentityStatusCode(identityException)
            : exception is BadHttpRequestException or JsonException
                ? StatusCodes.Status400BadRequest
                : StatusCodes.Status500InternalServerError;
        var code = identityException?.Code
            ?? (statusCode == StatusCodes.Status400BadRequest
                ? "MALFORMED_REQUEST"
                : "INTERNAL_SERVER_ERROR");
        var detail = identityException?.Message
            ?? (statusCode == StatusCodes.Status400BadRequest
                ? "The request body or parameters could not be read."
                : "An unexpected error occurred while processing the request.");

        if (statusCode == StatusCodes.Status500InternalServerError)
        {
            logger.LogError(exception,
                "Unhandled identity API error. Trace {TraceId}.",
                httpContext.TraceIdentifier);
        }
        else if (identityException is not null)
        {
            logger.LogWarning(
                "Identity request rejected with {ErrorCode} and HTTP {StatusCode}.",
                code,
                statusCode);
        }
        else
        {
            logger.LogWarning("Malformed identity API request. Trace {TraceId}.",
                httpContext.TraceIdentifier);
        }

        var retryAfterUtc = identityException switch
        {
            IdentityThrottleException throttle => throttle.RetryAfterUtc,
            IdentityLockedException locked => locked.RetryAfterUtc,
            _ => (DateTimeOffset?)null
        };
        if (retryAfterUtc is not null)
        {
            var retryAfter = Math.Max(
                1,
                (int)Math.Ceiling((retryAfterUtc.Value - DateTimeOffset.UtcNow).TotalSeconds));
            httpContext.Response.Headers.RetryAfter = retryAfter.ToString(
                System.Globalization.CultureInfo.InvariantCulture);
        }

        if (statusCode == StatusCodes.Status401Unauthorized)
        {
            httpContext.Response.Headers.WWWAuthenticate = "Bearer";
        }

        httpContext.Response.StatusCode = statusCode;
        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = new ProblemDetails
            {
                Status = statusCode,
                Detail = detail,
                Extensions =
                {
                    ["code"] = code
                }
            },
            Exception = exception
        });
    }

    private static int GetIdentityStatusCode(IdentityOperationException exception) => exception switch
    {
        IdentityUnauthorizedException => StatusCodes.Status401Unauthorized,
        IdentityForbiddenException => StatusCodes.Status403Forbidden,
        IdentityNotFoundException => StatusCodes.Status404NotFound,
        IdentityConflictException => StatusCodes.Status409Conflict,
        IdentityLockedException => StatusCodes.Status423Locked,
        IdentityThrottleException => StatusCodes.Status429TooManyRequests,
        IdentityDeliveryException => StatusCodes.Status503ServiceUnavailable,
        _ => StatusCodes.Status400BadRequest
    };
}
