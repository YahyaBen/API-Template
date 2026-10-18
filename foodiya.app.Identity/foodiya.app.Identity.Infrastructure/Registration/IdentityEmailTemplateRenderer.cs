using System.Collections.Concurrent;
using System.Net;
using System.Reflection;

namespace foodiya.app.Identity.Infrastructure.Registration;

internal static class IdentityEmailTemplateRenderer
{
    private static readonly Assembly TemplateAssembly = typeof(IdentityEmailTemplateRenderer).Assembly;
    private static readonly ConcurrentDictionary<string, string> Templates = new(StringComparer.Ordinal);

    public static string Render(string templateName, IReadOnlyDictionary<string, string> values)
    {
        var html = Templates.GetOrAdd(templateName, LoadTemplate);
        foreach (var (key, value) in values)
        {
            html = html.Replace($"{{{{{key}}}}}", WebUtility.HtmlEncode(value), StringComparison.Ordinal);
        }

        return html;
    }

    private static string LoadTemplate(string templateName)
    {
        var suffix = $".EmailTemplates.{templateName}.html";
        var resourceName = TemplateAssembly.GetManifestResourceNames()
            .SingleOrDefault(name => name.EndsWith(suffix, StringComparison.Ordinal))
            ?? throw new InvalidOperationException($"Embedded identity email template '{templateName}' was not found.");

        using var stream = TemplateAssembly.GetManifestResourceStream(resourceName)
            ?? throw new InvalidOperationException($"Embedded identity email template '{templateName}' could not be opened.");
        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
}
