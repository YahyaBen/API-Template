# foodiya.app.Identity setup

This .NET 10 solution was generated from the sandbox identity template. It uses
SQL Server, EF Core migrations, RSA-signed JWTs, OpenID Connect discovery/JWKS,
and persistent Docker storage.

The database name is exactly FOODIYA_APP_IDENTITY.

## 1. Prerequisites

Install the .NET 10 SDK, PowerShell 7, and Docker Desktop or Docker Engine with
Docker Compose v2. Verify them:

    dotnet --version
    pwsh --version
    docker version
    docker compose version

## 2. Generate local secrets

From this solution directory, run:

    pwsh ./scripts/New-LocalSecrets.ps1

This creates:

- .env with a random SQL Server administrator password, OTP pepper,
  refresh-token pepper, JWT key ID, outbox signing secret, and optional
  bootstrap-administrator password.
- secrets/jwt-private-key.pem with a 3072-bit PKCS#8 RSA private signing key.

The command never prints the values. Both paths are ignored by Git and excluded
from the Docker build context. Compose mounts the RSA key read-only at
/run/secrets/jwt-private-key.pem. The public JWKS endpoint never returns private
key material.

To intentionally rotate all local secrets:

    pwsh ./scripts/New-LocalSecrets.ps1 -Force

Rotation invalidates outstanding OTPs, refresh tokens, and JWT signatures. Stop
the service and plan the rotation before using -Force against shared data.

SMTP passwords, Google OAuth client IDs, and third-party webhook credentials
must come from those external providers and cannot be generated locally.

## 3. Review deployment values

Open .env and review IDENTITY_ISSUER. It must be the public HTTPS origin used by
clients. For example, after configuring your own HTTPS hostname:

    https://identity.example.com

BootstrapAdmin__Enabled is false by default. To create the first administrator
on an empty database, change it to true before the first start. A strong password
has already been generated. After the account exists, set the flag back to false
and restart the API.

## 4. Validate, build, and start Docker

    docker compose config --quiet
    docker compose up --build --detach
    docker compose ps

Follow startup and migration logs:

    docker compose logs --follow migrations api

The stack contains:

- sqlserver: SQL Server 2022 Developer on host port 14333.
- migrations: a one-shot EF Core migration container.
- api: the identity API on host port 8080, with persistent Data Protection keys.

The API starts only after SQL Server is healthy and migrations succeed.

Verify the running service:

    Invoke-RestMethod http://localhost:8080/health
    Invoke-RestMethod http://localhost:8080/.well-known/openid-configuration
    Invoke-RestMethod http://localhost:8080/.well-known/jwks.json

For remote testing, configure your own HTTPS reverse proxy or development
tunnel and set IDENTITY_ISSUER to its public origin. After it is available,
verify the public endpoints, replacing the example hostname:

    Invoke-RestMethod https://identity.example.com/health
    Invoke-RestMethod https://identity.example.com/.well-known/openid-configuration
    Invoke-RestMethod https://identity.example.com/.well-known/jwks.json

The discovery response advertises IDENTITY_ISSUER. The direct container port is
HTTP; production traffic should enter through an HTTPS reverse proxy.

## 5. Database and migrations

Every docker compose up runs dotnet ef database update against
FOODIYA_APP_IDENTITY before the API starts. The included migrations create the
identity schema and the Firebase Installation ID columns and unique index.

Run migrations manually:

    docker compose run --rm migrations

Check that the database exists:

    docker compose exec sqlserver bash -lc '/opt/mssql-tools18/bin/sqlcmd -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -C -Q "SELECT name FROM sys.databases WHERE name = '\''FOODIYA_APP_IDENTITY'\''"'

## 6. Persistent SQL Server volume

SQL data is stored in foodiya_app_identity_sql_data, mounted at /var/opt/mssql.
ASP.NET Data Protection keys are stored in
foodiya_app_identity_data_protection, mounted under /home/app/.aspnet. Both are
named Docker volumes and survive container replacement.

    docker volume inspect foodiya_app_identity_sql_data
    docker volume inspect foodiya_app_identity_data_protection
    docker compose down
    docker compose up --detach

docker compose down preserves the database. This command permanently deletes
the database volume and must only be used for an intentional reset:

    docker compose down --volumes

## 7. Run with the .NET CLI

Start only SQL Server:

    docker compose up --detach sqlserver

Load .env into the current PowerShell process:

    Get-Content .env | ForEach-Object {
        if ($_ -and -not $_.StartsWith('#')) {
            $name, $value = $_ -split '=', 2
            [Environment]::SetEnvironmentVariable($name, $value, 'Process')
        }
    }

Set the host-side connection and RSA paths:

    $env:ConnectionStrings__IdentityDatabase = "Server=localhost,14333;Database=FOODIYA_APP_IDENTITY;User ID=sa;Password=$env:SQL_SERVER_SA_PASSWORD;Encrypt=True;TrustServerCertificate=True;MultipleActiveResultSets=True"
    $env:Tokens__RsaPrivateKeyPath = (Resolve-Path ./secrets/jwt-private-key.pem).Path
    $env:Tokens__Issuer = "http://localhost:5000"

Restore tools, migrate, and run:

    dotnet tool restore
    dotnet ef database update --project foodiya.app.Identity.Infrastructure --startup-project foodiya.app.Identity.Api
    dotnet run --project foodiya.app.Identity.Api

In Development, OpenAPI is available at /openapi/v1.json and Scalar at
/scalar/v1.

## 8. Build and test

    dotnet build foodiya.app.Identity.slnx --configuration Release
    dotnet test --solution foodiya.app.Identity.slnx --configuration Release

## 9. Recreate from the template

From the parent workspace:

    dotnet new install ./SandBox.Identity/artifacts/packages/latest/SandBox.Identity.Templates.1.0.0.nupkg --force
    dotnet new sandbox-identity --name foodiya.app.Identity --output foodiya.app.Identity --dry-run

Do not generate over an existing project without first backing it up.
