namespace Foodiya.Domain.Extensions;

/// <summary>
/// Generates unique <c>Code</c> values for entities that rely on
/// <c>AFTER INSERT</c> SQL triggers (e.g. <c>TR_AppUser_GenerateCode</c>).
/// <para>
/// Because the <c>Code</c> column is <c>NOT NULL</c>, the value must be
/// supplied <b>before</b> the INSERT statement—triggers fire too late.
/// The generated format mirrors the trigger logic:
/// <c>{PREFIX}_{8-char-uppercase-GUID-fragment}</c>.
/// </para>
/// </summary>
public static class EntityCodeGenerator
{
    /// <summary>
    /// Generates a code in the form <c>{prefix}_{8-char GUID}</c>,
    /// e.g. <c>USR_A1B2C3D4</c>.
    /// </summary>
    public static string For(string prefix)
        => $"{prefix}_{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";

    /// <summary>
    /// Generates a date-based code in the form <c>{prefix}_{yyyyMMdd}</c>,
    /// e.g. <c>DRS_20260408</c>.  Used by <c>DailyRecipeStats</c>.
    /// </summary>
    public static string ForDate(string prefix, DateTime date)
        => $"{prefix}_{date:yyyyMMdd}";

    /// <summary>
    /// Generates a slug-based code from a city slug (uppercased, hyphens → underscores).
    /// Matches the <c>TR_MoroccanCity_GenerateCode</c> trigger logic.
    /// </summary>
    public static string FromSlug(string slug)
        => slug.Replace('-', '_').ToUpperInvariant();
}
