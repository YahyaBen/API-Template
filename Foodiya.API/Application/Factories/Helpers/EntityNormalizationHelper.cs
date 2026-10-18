using Foodiya.Domain.Exceptions;
using Foodiya.Domain.Extensions;

namespace Foodiya.Application.Factories.Helpers;

/// <summary>
/// Shared normalization helpers used by entity factories.
/// Mirrors the NormalizeRequired / NormalizeOptional pattern from the services
/// so that factories produce the exact same output.
/// </summary>
internal static class EntityNormalizationHelper
{
    public static string Required(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new FoodiyaBadRequestException($"{fieldName} cannot be empty.");
        return value.Trim();
    }

    public static string? Optional(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    public static string Code(string code, string fieldName)
        => Required(code, fieldName).ToUpperInvariant();

    public static string Slug(string name)
        => Required(name, nameof(name)).ToSlug();

    public static string SlugWithSuffix(string title)
        => title.ToSlug() + "-" + Guid.NewGuid().ToString("N")[..8];
}
