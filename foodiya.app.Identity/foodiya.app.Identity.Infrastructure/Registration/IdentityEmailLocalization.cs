using System.Globalization;
using System.Reflection;
using System.Resources;

namespace foodiya.app.Identity.Infrastructure.Registration;

internal static class IdentityEmailLocalization
{
    private static readonly ResourceManager Messages = new(
        "foodiya.app.Identity.Infrastructure.Resources.Localization.EmailMessages",
        typeof(IdentityEmailLocalization).Assembly);

    public static string NormalizeLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return "en";
        }

        var normalized = language.Trim().Split('-', '_')[0].ToLowerInvariant();
        return normalized is "fr" or "ar" ? normalized : "en";
    }

    public static string Direction(string language) => language == "ar" ? "rtl" : "ltr";

    public static string TextAlign(string language) => language == "ar" ? "right" : "left";

    public static string Get(string language, string key, params object[] arguments)
    {
        var culture = CultureInfo.GetCultureInfo(NormalizeLanguage(language));
        var format = Messages.GetString(key, culture)
            ?? Messages.GetString(key, CultureInfo.GetCultureInfo("en"))
            ?? throw new InvalidOperationException($"Identity email resource '{key}' was not found.");
        return arguments.Length == 0 ? format : string.Format(culture, format, arguments);
    }
}
