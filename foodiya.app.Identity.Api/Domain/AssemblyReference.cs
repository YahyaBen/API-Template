namespace foodiya.app.Identity.Domain;

/// <summary>Provides access to the domain assembly without coupling callers to a domain type.</summary>
public static class AssemblyReference
{
    public static System.Reflection.Assembly Assembly => typeof(AssemblyReference).Assembly;
}
