namespace Foodiya.Domain.Constants;

/// <summary>
/// Share channel constants — stored in RecipeShare.ShareChannel column.
/// </summary>
public static class ShareChannelConstants
{
    public const string Application = "APPLICATION";
    public const string Link = "LINK";
    public const string Email = "EMAIL";
    public const string WhatsApp = "WHATSAPP";
    public const string Sms = "SMS";

    public const string ValidationPattern = "(?i)^(APPLICATION|LINK|EMAIL|WHATSAPP|SMS)$";
}
