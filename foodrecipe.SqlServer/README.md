# Foodiya database scripts

These SQL scripts belong to the Foodiya business database. Identity has a
separate database managed by EF Core migrations in
`../foodiya.app.Identity/foodiya.app.Identity.Infrastructure/Persistence/Migrations/`.

Create an empty Foodiya SQL Server or Azure SQL database. Review and execute the
following scripts against that target with SSMS, Azure's query editor, or
`sqlcmd` with error handling enabled. The base script includes example reference
data and demo users; review that data before using a shared environment.

Apply this sequence for a fresh database:

1. `FoodRecipeApp_SQL.sql`
2. `Migration_SoftDelete.sql`
3. `Migration_RBAC_AppUserRole.sql`
4. `Migration_RefreshToken.sql`
5. `Migration_GoogleAuth_AppUserExternalLogin.sql`
6. `Migration_SaaS_Referential_Standardization.sql`
7. `Migration_Uuid_Code_AllTables.sql`
8. `Migration_AddColor_ChefDifficultyRecipeAppUserRecipeNutrition.sql`
9. `Migration_ImageUrl_AllTables.sql`
10. `Migration_DailyRecipeStats.sql`
11. `Migration_StatsDate_DateToDatetime2.sql`
12. `Migration_IdentityInboxMessage.sql`
13. `Migration_AppUserEmail256.sql`
14. `Migration_IdentityProfileUnicodeNames.sql`

The schema comes from the existing Foodiya project. This sequence needs a fresh
database smoke test in your SQL environment before deployment. For an existing
database, determine which changes are already applied before running scripts.

`Migration_LocalAuth_DemoUserPasswords.sql` is an optional legacy demo script;
the current APIs authenticate through the separate Identity service.
`Drop_FoodRecipeApp_SQL.sql` is an explicit destructive reset utility and is not
part of setup or normal deployment.

Set `ConnectionStrings__DefaultConnection` for the API. Before enabling
`IdentityEvents__Enabled`, the inbox, email-length, and Unicode-name migrations
must be applied. The API does not apply these migrations on startup.
