namespace Foodiya.Infrastructure.Data;

/// <summary>
/// SQL Server-specific error numbers. Lives in Infrastructure because
/// these are vendor-specific constants, not domain knowledge.
/// </summary>
internal static class SqlErrorNumbers
{
    public const int UniqueConstraintViolation = 2627;
    public const int UniqueIndexViolation = 2601;
}
