using Foodiya.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Foodiya.API.Middleware;

/// <summary>
/// Global exception handler that maps domain exceptions to proper HTTP ProblemDetails responses.
/// Implements IExceptionHandler (ASP.NET Core 8+) for a clean pipeline integration.
/// </summary>
public sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            FoodiyaNotFoundException        => (StatusCodes.Status404NotFound,            "Resource Not Found"),
            FoodiyaNullArgumentException     => (StatusCodes.Status404NotFound,            "Resource Not Found"),
            FoodiyaUnauthorizedException     => (StatusCodes.Status401Unauthorized,        "Unauthorized"),
            FoodiyaBadRequestException       => (StatusCodes.Status400BadRequest,          "Bad Request"),
            FoodiyaForbiddenException        => (StatusCodes.Status403Forbidden,           "Forbidden"),
            FoodiyaValueAlreadyExistsException => (StatusCodes.Status409Conflict,          "Duplicate Resource"),
            FoodiyaBaseException             => (StatusCodes.Status400BadRequest,          "Bad Request"),
            _                                   => (StatusCodes.Status500InternalServerError, "Internal Server Error")
        };

        _logger.LogError(exception, "Unhandled exception — {Title}: {Message}", title, exception.Message);

        var detail = exception is FoodiyaBaseException
            ? exception.Message
            : "An unexpected error occurred.";

        // In Development, always expose the full exception chain for debugging
        var env = httpContext.RequestServices.GetService<IHostEnvironment>();
        if (env?.IsDevelopment() == true)
            detail = exception.ToString();

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
        return true;
    }
}
