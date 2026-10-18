namespace Foodiya.Domain.Constants;

public static class RecipeVisibilityConstants
{
    public const string Public = "PUBLIC";
    public const string Private = "PRIVATE";
    public const string Unlisted = "UNLISTED";
    public const string ValidationPattern = "(?i)^(PUBLIC|PRIVATE|UNLISTED)$";
}
