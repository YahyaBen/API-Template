using System.Text;
using System.Text.RegularExpressions;

namespace Foodiya.Domain.Extensions;

public static partial class StringExtensions
{
    /// <summary>
    /// Converts a title into a URL-friendly slug.
    /// </summary>
    public static string ToSlug(this string text)
    {
        var slug = text.ToLowerInvariant().Trim();
        slug = Regex.Replace(slug, @"[àáâãäå]", "a");
        slug = Regex.Replace(slug, @"[èéêë]", "e");
        slug = Regex.Replace(slug, @"[ìíîï]", "i");
        slug = Regex.Replace(slug, @"[òóôõö]", "o");
        slug = Regex.Replace(slug, @"[ùúûü]", "u");
        slug = Regex.Replace(slug, @"[ç]", "c");
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"[\s-]+", "-").Trim('-');
        return slug;
    }
}
