# API Template: two .NET projects

This repository contains exactly two .NET 10 projects:

| Project | Responsibility |
| --- | --- |
| `foodiya.app.Identity.Api` | Registration, email OTP, login, JWTs, refresh tokens, roles, sessions, devices, and account recovery |
| `Foodiya.API` | Recipes, ingredients, chefs, reference data, nutrition, and social endpoints; validates Identity JWTs |

Each API contains its application, domain, infrastructure, and messaging code as
folders. There are no supporting library, Azure Functions, or test projects.
SQL, deployment configuration, and scripts are supporting files, not additional
.NET projects. Existing namespaces and HTTP contracts are preserved.

## Build

Install a .NET 10 SDK and run from the repository root:

```powershell
dotnet tool restore
dotnet build API-Template.slnx --configuration Release
```

Package versions are managed in `Directory.Packages.props`. Both APIs build
independently and have no project references.

## Configure the databases

The services own separate SQL Server databases.

1. Create the Foodiya database using [the SQL guide](foodrecipe.SqlServer/README.md).
2. Copy `Foodiya.API/appsettings.Local.example.json` to
   `Foodiya.API/appsettings.Local.json` and configure its connection string.
3. Identity Development defaults to Windows LocalDB, database `FOODIYA_APP_IDENTITY`.
   For another server, set `ConnectionStrings__IdentityDatabase` in the terminal
   used for migrations and startup.
4. Apply Identity's migrations, now included inside the Identity API:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet tool run dotnet-ef database update `
  --project foodiya.app.Identity.Api `
  --startup-project foodiya.app.Identity.Api
```

The APIs do not apply database migrations during normal startup.

## Run both APIs

Use two terminals from the repository root:

```powershell
# Terminal 1
dotnet run --project foodiya.app.Identity.Api
```

```powershell
# Terminal 2
dotnet run --project Foodiya.API --launch-profile http
```

| Endpoint | URL |
| --- | --- |
| Identity Scalar, Development only | http://localhost:5000/scalar/v1 |
| Foodiya Scalar, Development only | http://localhost:5135/scalar/v1 |
| Foodiya Swagger UI | http://localhost:5135/swagger |
| Identity discovery | http://localhost:5000/.well-known/openid-configuration |
| Identity public signing keys | http://localhost:5000/.well-known/jwks.json |
| Health | `/health` on either service |

The Foodiya local example uses Identity authority and issuer
`http://localhost:5000`, with audience `foodiya.app.Identity.Clients`.
Register and confirm the email OTP through Identity, then log in and pass the
access token to Foodiya as `Authorization: Bearer <access-token>`.
Foodiya validates RS256 signatures through Identity's discovery and public keys.
Development email delivery writes OTPs to local logs.

On Windows with PowerShell 7.4 or later, after configuring the connections:

```powershell
./scripts/Start-LocalApis.ps1
./scripts/Test-LocalApis.ps1
```

The launcher disables RabbitMQ workers for local HTTP verification. Logs and
process IDs go to ignored `artifacts/local-run/`. The smoke script creates a
synthetic account in the local test databases and checks OTP confirmation,
login, authorized reads, role checks, refresh rotation, logout, and invalid JWT
rejection. It does not print passwords, OTPs, or tokens.

## Containers and messaging

- The root `Dockerfile` and Compose file build Foodiya.API. Copy the root
  `.env.example` to `.env` and configure SQL and a reachable Identity URL.
- Identity's `Dockerfile` and Compose file are inside `foodiya.app.Identity.Api/`
  and use the repository root as their build context. Generate local secrets with
  `./foodiya.app.Identity.Api/scripts/New-LocalSecrets.ps1`; configure production
  SMTP and issuer settings before starting that stack.
- Optional RabbitMQ setup is documented in
  [deployment/rabbitmq](deployment/rabbitmq/README.md).

The standalone Compose stacks both use host port 8080. Run separately or use
Identity's RabbitMQ override, which moves it to 8081. JWT discovery must be
reachable from the consuming API container and use the same issuer as clients.
The APIs carry matching messaging source files under their `Messaging/` folders;
update both copies together when changing the event contract.

## Publish and deploy

```powershell
dotnet publish Foodiya.API/Foodiya.API.csproj --configuration Release `
  --output artifacts/publish/api
dotnet publish foodiya.app.Identity.Api/foodiya.app.Identity.Api.csproj `
  --configuration Release --output artifacts/publish/identity
```

Deploy as two separate services, for example two Azure App Services with separate
SQL databases. Supply database connections, HTTPS issuer/audience, SMTP settings,
RSA signing material, and independent OTP/refresh secrets through the deployment
environment. See [Identity operations](docs/identity-operations.md).
Development and local settings are excluded from publish output. Credentials,
private keys, and artifacts are ignored by Git.

## Verification

GitHub Actions builds and publishes both projects, then checks HTTP startup,
Scalar, OpenAPI, discovery, and anonymous access rejection without a database.
Use `scripts/Test-LocalApis.ps1` with local SQL for full authentication checks.

Validated after consolidation on 2026-10-18: both projects built in Release with
zero warnings and errors, both published successfully, and all 41 live HTTP
checks passed against isolated local SQL Server databases. RabbitMQ and Azure
deployment were not exercised in this validation.
