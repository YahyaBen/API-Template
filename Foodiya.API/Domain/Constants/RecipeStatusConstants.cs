namespace Foodiya.Domain.Constants;

public static class RecipeStatusConstants
{
    public const string Draft = "DRAFT";
    public const string Published = "PUBLISHED";
    public const string Archived = "ARCHIVED";
    public const string ValidationPattern = "(?i)^(PUBLISHED|DRAFT|ARCHIVED)$";
}
