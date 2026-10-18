# Foodiya development database

From the repository root, create a local database with one command:

```powershell
./foodrecipe.SqlServer/Initialize-Database.ps1
```

Requires a local SQL Server, Windows authentication, and `sqlcmd`. This creates
`FoodiyaLocal` on server `.` and runs `Initialize.sql`: the complete schema and
sample data. Existing databases are never replaced.

For another local server or a fresh database name:

```powershell
./foodrecipe.SqlServer/Initialize-Database.ps1 -Server . -DatabaseName FoodiyaDev
```

Set the API connection in your terminal before starting Foodiya:

```powershell
$env:ConnectionStrings__DefaultConnection = 'Server=.;Database=FoodiyaLocal;Integrated Security=True;Encrypt=True;TrustServerCertificate=True'
```

Alternatively, put it in the ignored `Foodiya.API/appsettings.Local.json`.
Logs go to ignored `artifacts/database/`.

`Initialize.sql` replaces the previous 14-script setup with one initial
baseline. You can also run it once against an empty SQL Server or Azure SQL
database using SSMS, Azure's query editor, or `sqlcmd -b -f 65001`. It is for
fresh databases; existing databases already initialized by the old scripts
need no changes. The previous scripts remain available in Git history.

Sample users and recipes are development data. Sign in through Identity,
which owns a separate database and keeps its EF Core migrations in
`foodiya.app.Identity.Api/Infrastructure/Persistence/Migrations/`.
The APIs do not initialize or reset databases on startup.
