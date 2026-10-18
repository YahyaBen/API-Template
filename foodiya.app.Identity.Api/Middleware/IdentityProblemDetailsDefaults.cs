using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;

namespace foodiya.app.Identity.Api.Middleware;

internal static class IdentityProblemDetailsDefaults
{
    public static void Customize(ProblemDetailsContext context)
    {
        var problem = context.ProblemDetails;
        var status = problem.Status ?? context.HttpContext.Response.StatusCode;
        var code = ResolveCode(context, status);
        problem.Status = status;
        problem.Title = IdentityProblemLocalizer.GetTitle(status);
        problem.Detail = IdentityProblemLocalizer.GetDetail(
            code,
            problem.Detail ?? GetDefaultDetail(code, status));
        problem.Instance ??= context.HttpContext.Request.Path;
        problem.Type ??= $"urn:foodiya-app-identity:problem:{code.ToLowerInvariant().Replace('_', '-')}";
        problem.Extensions["code"] = code;
        problem.Extensions["traceId"] = Activity.Current?.Id ?? context.HttpContext.TraceIdentifier;

        if (problem is HttpValidationProblemDetails validationProblem)
        {
            IdentityProblemLocalizer.LocalizeValidationErrors(validationProblem);
        }
    }

    private static string ResolveCode(ProblemDetailsContext context, int status)
    {
        if (context.ProblemDetails.Extensions.TryGetValue("code", out var configuredCode)
            && configuredCode is string code
            && !string.IsNullOrWhiteSpace(code))
        {
            return code;
        }

        if (context.ProblemDetails is HttpValidationProblemDetails)
        {
            return "VALIDATION_FAILED";
        }

        if (context.Exception is BadHttpRequestException)
        {
            return "MALFORMED_REQUEST";
        }

        return status switch
        {
            StatusCodes.Status400BadRequest => "BAD_REQUEST",
            StatusCodes.Status401Unauthorized => "AUTHENTICATION_REQUIRED",
            StatusCodes.Status403Forbidden => "AUTHORIZATION_FORBIDDEN",
            StatusCodes.Status404NotFound => "RESOURCE_NOT_FOUND",
            StatusCodes.Status405MethodNotAllowed => "METHOD_NOT_ALLOWED",
            StatusCodes.Status409Conflict => "CONFLICT",
            StatusCodes.Status415UnsupportedMediaType => "UNSUPPORTED_MEDIA_TYPE",
            StatusCodes.Status429TooManyRequests => "RATE_LIMIT_EXCEEDED",
            StatusCodes.Status500InternalServerError => "INTERNAL_SERVER_ERROR",
            StatusCodes.Status503ServiceUnavailable => "SERVICE_UNAVAILABLE",
            _ => $"HTTP_{status}"
        };
    }

    private static string GetDefaultDetail(string code, int status) => code switch
    {
        "VALIDATION_FAILED" => "One or more request fields are invalid.",
        "MALFORMED_REQUEST" => "The request body or parameters could not be read.",
        "AUTHENTICATION_REQUIRED" => "Authentication is required to access this resource.",
        "AUTHORIZATION_FORBIDDEN" => "The authenticated account cannot access this resource.",
        "RESOURCE_NOT_FOUND" => "The requested resource was not found.",
        "METHOD_NOT_ALLOWED" => "The HTTP method is not supported for this resource.",
        "UNSUPPORTED_MEDIA_TYPE" => "The request content type is not supported.",
        "INTERNAL_SERVER_ERROR" => "An unexpected error occurred while processing the request.",
        _ => ReasonPhrases.GetReasonPhrase(status)
    };
}
