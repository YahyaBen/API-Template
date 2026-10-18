# API Template: Identity and Foodiya API

A self-contained .NET 10 starter assembled from the integrated Foodiya services.
It includes the Identity API, the main business API, their supporting projects,
RabbitMQ integration, database scripts, tests, and optional Azure Functions.
Existing namespaces are retained so the integrations continue to work.

## Included components

| Component | Responsibility |
| --- | --- |
| `foodiya.app.Identity/` | Registration, email OTP, JWTs, refresh tokens, roles, devices, sessions, account recovery, and Identity tests |
| `Foodiya.API/` | Recipe, ingredient, chef, reference-data, nutrition, and social HTTP endpoints |
| `Foodiya.Application/` | Business services and request/response models |
| `Foodiya.Domain/` | Models, rules, interfaces, and query specifications |
| `Foodiya.Infrastructure/` | SQL Server persistence and Identity profile synchronization |
| `foodiya.app.Identity/foodiya.app.Messaging/` | Shared RabbitMQ event contracts, routing, connections, and publishing |
| `foodiya.app.RabbitMq/` | Local broker configuration and scripts |
| `foodrecipe.SqlServer/` | Foodiya database creation and migration scripts |
| `Foodiya.Functions/` | Optional daily statistics, calorie calculations, and a simulated welcome-email worker |

`API-Template.slnx` includes all 13 application, library, and test projects.
The existing `Foodiya.API.sln`, Identity solution, and integration-test solution
are also included for focused work. There are no references to projects outside
this repository. The other Identity copies and Sandbox.Repository are not needed
by these services and are not included.

## Build

Install a compatible .NET 10 SDK. From the repository root:

```powershell
dotnet tool restore
dotnet restore API-Template.slnx
dotnet build API-Template.slnx --configuration Release --no-restore
```

## Configure the databases

Identity and Foodiya own separate SQL Server databases.

1. Create a Foodiya database and apply the scripts described in
   [the database guide](foodrecipe.SqlServer/README.md).
2. Copy `Foodiya.API/appsettings.Local.example.json` to
   `Foodiya.API/appsettings.Local.json` and provide its database connection.
3. For Identity, Development defaults to Windows LocalDB and database
   `FOODIYA_APP_IDENTITY`. For another SQL Server, set
   `ConnectionStrings__IdentityDatabase` in the terminal used for migrations and
   Identity startup.
4. Apply Identity's EF Core migrations:

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
dotnet tool run dotnet-ef database update `
  --project foodiya.app.Identity/foodiya.app.Identity.Infrastructure `
  --startup-project foodiya.app.Identity/foodiya.app.Identity.Api
```

The APIs do not create or migrate the Foodiya database during normal startup.

## Run the APIs locally

Use two terminals from the repository root:

```powershell
# Terminal 1: Identity
dotnet run --project foodiya.app.Identity/foodiya.app.Identity.Api
```

```powershell
# Terminal 2: Foodiya
dotnet run --project Foodiya.API --launch-profile http
```

| Service | Local URL |
| --- | --- |
| Identity API | `http://localhost:5000` |
| Identity Scalar documentation, Development only | `http://localhost:5000/scalar/v1` |
| Identity discovery | `http://localhost:5000/.well-known/openid-configuration` |
| Identity public signing keys | `http://localhost:5000/.well-known/jwks.json` |
| Foodiya Scalar documentation, Development only | `http://localhost:5135/scalar/v1` |
| Foodiya Swagger UI | `http://localhost:5135/swagger` |
| Foodiya liveness | `http://localhost:5135/health` |

The local example configures Foodiya's JWT authority and issuer as
`http://localhost:5000`, with audience `foodiya.app.Identity.Clients`.
Authenticate against Identity, then pass the returned access token to Foodiya as
`Authorization: Bearer <access-token>`. Foodiya validates RS256 signatures through
Identity's discovery/JWKS metadata; it never needs Identity's private key.

Development email delivery logs OTPs instead of sending real email. This Identity
version confirms registration first and uses a separate login request to obtain
tokens. It does not include Sandbox's combined `isOtpValidated` login-response
contract. See [the Identity API guide](foodiya.app.Identity/README.md).

On Windows with PowerShell 7.4 or later, after configuring the databases you can
start both APIs in the background and run the live smoke test:

```powershell
./scripts/Start-LocalApis.ps1
./scripts/Test-LocalApis.ps1
```

The launcher uses the configured connection strings, disables RabbitMQ workers
for this local HTTP test, and binds both APIs to localhost. It refuses occupied
ports. Process IDs and logs are stored under `artifacts/local-run/`, outside Git.
The smoke test creates a synthetic account, reads its Development OTP from the
local log, and tests authentication, authorized collection reads, role checks,
refresh rotation, logout, and invalid JWT rejection. It does not print passwords,
OTPs, or tokens. Run it only against the intended local test databases.

## Test

Run the tests that do not require a live broker or SQL Server:

```powershell
dotnet test --solution API-Template.slnx --configuration Release --no-build `
  --filter "TestCategory!=RabbitMQ&TestCategory!=SqlServer"
```

For live RabbitMQ and SQL Server tests, use
[the messaging runbook](foodiya.app.RabbitMq/README.md) and
`foodiya.app.Identity/scripts/Test-SqlServer.ps1`. These tests need explicitly
configured disposable dependencies; they are not included in the command above.

## Docker and messaging

The stacks can be started separately using their included Compose files:

1. Generate new Identity credentials with
   `./foodiya.app.Identity/scripts/New-LocalSecrets.ps1`.
2. Follow [Identity setup](foodiya.app.Identity/SETUP.md) to configure its issuer
   and SMTP provider before starting its production-mode Compose stack.
3. For the main API's Compose stack, copy `.env.example` to `.env` and configure
   a database and an Identity HTTPS URL reachable from the API container.
4. Follow [RabbitMQ setup](foodiya.app.RabbitMq/README.md) before enabling the
   publisher or consumer. Identity profile migrations must already be applied.

The default standalone API and Identity Compose stacks both use host port 8080;
run them separately or use the documented RabbitMQ override, which moves
Identity to 8081. These files are separate stacks, not a single turnkey Compose
deployment. For container-to-container JWT discovery, the configured Identity
URL must be reachable inside the main API container as well as by clients.

Local `.env` files, SQL passwords, private keys, production settings, and build
artifacts are excluded from Git. Generated credentials from the original
workspace have not been copied into this repository.

## Publish

Publish the APIs independently:

```powershell
dotnet publish Foodiya.API/Foodiya.API.csproj --configuration Release `
  --output artifacts/publish/api
dotnet publish foodiya.app.Identity/foodiya.app.Identity.Api/foodiya.app.Identity.Api.csproj `
  --configuration Release --output artifacts/publish/identity
```

Provide database connections, HTTPS issuer/audience settings, SMTP configuration,
RSA signing material, and independent OTP/refresh-token secrets through the
deployment environment. Development settings are excluded from published APIs.
See [operations](foodiya.app.Identity/docs/operations.md) for Identity deployment
and secret-rotation considerations.

For optional Azure Functions, copy `Foodiya.Functions/local.settings.example.json`
to `local.settings.json`, configure SQL and storage, and follow
[the Functions instructions](docs/foodiya-api.md#azure-functions-foodiyafunctions).
The welcome-email worker currently simulates delivery and requires a real
provider before use.

## Changes made while assembling this starter

- Added a combined solution and repository-level setup documentation.
- Kept the existing Foodiya and Identity integration and test suites.
- Disabled the original application's Google OAuth client ID in Development.
- Excluded Development settings from API publish output.
- Pinned `System.Security.Cryptography.Pkcs` to 10.0.13 to resolve the dependency
  audit failures described in [Microsoft's advisory](https://github.com/advisories/GHSA-846f-f764-76mx).
- Added GitHub Actions checks for build, tests without external dependencies,
  and publishing both APIs on pushes and pull requests.

This is a Foodiya-based starter with domain-specific examples, not a fully
renamed domain-neutral template. Production readiness requires deployment and
dependency verification in the target environment.

## Assembly validation

Validated on 2026-10-18 with .NET SDK 10.0.401:

- All 13 projects built in Release with zero warnings and zero errors.
- All 130 selected tests without external dependencies passed.
- Both APIs published successfully without Development or local settings.
- All 26 project references resolved inside this repository.
- All five Docker Compose configurations validated with example values, without
  loading local credentials or starting containers.

Further local validation on 2026-10-18:

- Four additional SQL Server integration tests passed.
- Foodiya initialized successfully from an empty SQL Server database after
  correcting the statistics-table migration order; all Identity migrations applied.
- Both Scalar interfaces rendered in the browser and returned HTTP 200 for
  interactive health requests.
- All 41 live smoke checks passed, including Identity JWT access to 19 Foodiya
  collection endpoints and the reference-data endpoint.

The APIs were run against isolated local SQL Server databases. RabbitMQ and
Azure deployment were not tested; the Docker daemon was not available.
