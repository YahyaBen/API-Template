using System.Globalization;
using System.Resources;
using Microsoft.AspNetCore.Mvc;

namespace foodiya.app.Identity.Api.Middleware;

internal static class IdentityProblemLocalizer
{
    private static readonly ResourceManager Messages = new(
        "foodiya.app.Identity.Api.Resources.Localization.IdentityErrorMessages",
        typeof(IdentityProblemLocalizer).Assembly);

    public static string GetTitle(int statusCode)
    {
        var key = statusCode switch
        {
            StatusCodes.Status400BadRequest => "Title_400",
            StatusCodes.Status401Unauthorized => "Title_401",
            StatusCodes.Status403Forbidden => "Title_403",
            StatusCodes.Status404NotFound => "Title_404",
            StatusCodes.Status405MethodNotAllowed => "Title_405",
            StatusCodes.Status409Conflict => "Title_409",
            StatusCodes.Status415UnsupportedMediaType => "Title_415",
            StatusCodes.Status423Locked => "Title_423",
            StatusCodes.Status429TooManyRequests => "Title_429",
            StatusCodes.Status500InternalServerError => "Title_500",
            StatusCodes.Status503ServiceUnavailable => "Title_503",
            _ => "Title_Default"
        };

        return Get(key, "Request failed");
    }

    public static string GetDetail(string code, string fallback)
    {
        var localized = Messages.GetString($"Detail_{code}", CultureInfo.CurrentUICulture);
        if (!string.IsNullOrWhiteSpace(localized))
        {
            return localized;
        }

        return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en"
            ? fallback
            : Get("Detail_Default", fallback);
    }

    public static void LocalizeValidationErrors(HttpValidationProblemDetails validationProblem)
    {
        var message = Get("Validation_FieldInvalid", "The value for this field is invalid.");
        foreach (var field in validationProblem.Errors.Keys.ToArray())
        {
            validationProblem.Errors[field] = [message];
        }
    }

    private static string Get(string key, string fallback) =>
        Messages.GetString(key, CultureInfo.CurrentUICulture) is { Length: > 0 } localized
            ? localized
            : fallback;
}
