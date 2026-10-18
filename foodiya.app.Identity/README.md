# foodiya.app.Identity Service

RabbitMQ delivery is available through `Outbox:Transport=RabbitMq`. See the
[RabbitMQ setup and verification guide](../foodiya.app.RabbitMq/README.md) for the
broker, publisher, profile consumer, and migration commands.

The Foodiya API consumes this service's discovery/JWKS metadata and RS256 tokens.
Set its `Authentication:Jwt:Authority`, `Issuer`, and `Audience` to match this
service. Identity-only tests use the main solution; cross-service tests use
`foodiya.app.Identity.Integration.slnx`. See the RabbitMQ runbook for the additional
SQL Server checks and required profile migrations.

Reusable, domain-neutral identity-service template for .NET 10 applications.

## Current state

Steps 1 through 10 are complete. The service now includes the reusable solution
and template foundation, GUID-based ASP.NET Core Identity persistence, and an
end-to-end registration workflow with hashed email OTP confirmation. Active
accounts can sign in through locked-down JWT sessions with rotating refresh tokens.
Role policies, current-user discovery, protected role administration, and
opt-in Google ID-token authentication are also available.
Password recovery, password history, session logout, and confirmed account
deletion are also implemented. Operational hardening adds SMTP delivery,
leased webhook outbox dispatch, scheduled retention cleanup, rate limiting,
English, French, and Arabic Problem Details messages, localized RTL-capable
identity emails, structured audit logs, and OpenTelemetry.

## Planned implementation steps

1. Foundation and template metadata. **Complete**
2. Identity domain model and EF Core persistence. **Complete**
3. Registration and email OTP confirmation. **Complete**
4. Login, lockout, JWT access tokens, and refresh-token rotation. **Complete**
5. Roles, authorization policies, current-user endpoint, and role administration.
   **Complete**
   - Optional Google authentication is available when client IDs are configured.
6. Password recovery, password history, logout, and account deletion. **Complete**
7. Device verification, device sessions, and Firebase installation registration. **Complete**
8. Operations and hardening: localization, rate limiting, production email
   providers, outbox dispatch, scheduled token/session/OTP cleanup, structured
   audit and security logs, observability, secret/key rotation guidance, and
   retention/purging of deleted-account records and historical event payloads.
   **Complete**
9. Unit, integration, architecture, and template matrix tests. **Complete**
10. Template packaging and foodiya.app.Identity integration. **Complete**

## Build

```powershell
dotnet build foodiya.app.Identity.slnx
```

## Database migrations

```powershell
dotnet tool restore
dotnet tool run dotnet-ef database update `
  --project foodiya.app.Identity.Infrastructure `
  --startup-project foodiya.app.Identity.Api
```

## Registration API

- `POST /api/auth/register` creates a pending `USER` account and OTP challenge.
- `POST /api/auth/register/confirm` consumes the OTP and activates the account.
- `POST /api/auth/register/resend` invalidates the previous OTP and enforces a cooldown.

OTP values are generated cryptographically and only an HMAC-SHA256 hash is
stored. In Development, the email adapter writes the OTP to the application log.
Before running in another environment, provide at least a 32-character secret:

```powershell
$env:Registration__OtpPepper = "replace-with-a-secret-from-your-secret-store"
```

Development OTP logging is explicit and development-only. Production SMTP is
available through `EmailDelivery:Provider=Smtp`; non-development environments
fail closed until a provider is configured. SMTP messages are multipart emails
with an accessible plain-text fallback and embedded, logo-free foodiya.app.Identity HTML
templates for verification codes, device verification, deletion, and welcome
notifications. See [the operations runbook](docs/operations.md).

Example requests are available in
`foodiya.app.Identity.Api/foodiya.app.Identity.Api.http`. OpenAPI is exposed at
`/openapi/v1.json` while the service runs in Development.

## Authentication API

- `POST /api/auth/login` authenticates by email or user name, records device
  metadata, creates a bounded session, and returns a JWT/refresh-token pair.
- `POST /api/auth/google` validates a Google ID token, links or creates the
  account, records device metadata, and returns the same JWT/refresh-token pair.
- `POST /api/auth/token/refresh` rotates the refresh token atomically and returns
  a replacement pair.

Access tokens expire after 15 minutes by default. Refresh tokens are opaque,
HMAC-hashed in storage, tied to a device session, and expire with that session.
Reusing a rotated refresh token revokes every active refresh token in the session.
Five failed passwords temporarily lock the account for 15 minutes.

Google authentication is disabled until at least one OAuth client ID is supplied.
Configure every web, Android, or iOS client whose ID tokens this backend accepts:

```powershell
$env:Authentication__Google__ClientIds__0 = "your-web-client-id.apps.googleusercontent.com"
```

The client must send the Google-issued ID token—not a plain Google user ID—to
`POST /api/auth/google`. The backend verifies Google's signature, issuer,
expiration, and audience, then keys the external login by Google's stable `sub`
claim. Automatic linking to an existing email account is allowed only when
Google is authoritative for that email (Gmail or a Google Workspace hosted
domain); other existing accounts must be linked through a separately
authenticated flow.

Production deployments must obtain these secrets from their secret store:

```powershell
$env:Tokens__RsaPrivateKeyPath = (Resolve-Path ./secrets/jwt-private-key.pem).Path
$env:Tokens__KeyId = "foodiya-app-identity-2026-09"
$env:Tokens__RefreshTokenPepper = "replace-with-an-independent-random-pepper"
```

The private key must be RSA with at least 2048 bits. Use a new key ID whenever the
key changes. The issuer, audience, token lifetimes, and lockout settings are
configurable; development-only values are supplied in
`appsettings.Development.json`.

## Authorization API

- `GET /api/users/me` returns the authenticated account, current roles, session,
  and device metadata.
- `GET /api/admin/roles` returns the canonical role catalog.
- `GET /api/admin/users/{userId}/roles` returns a user's assigned roles.
- `PUT /api/admin/users/{userId}/roles` replaces a user's roles. It requires the
  `SUPER_ADMIN` policy.

The role hierarchy is `USER`, `CHEF`, `ADMIN`, and `SUPER_ADMIN`. Chef policy
accepts chefs and administrators; admin policy accepts admins and super admins;
super-admin policy accepts only super admins. Role changes rotate the target
user's security stamp and revoke all active sessions and refresh tokens, so stale
permissions cannot remain active. The last super administrator cannot be demoted.

For a new zero-user deployment, apply the database migration, start one instance
with the following secret-store values, then immediately disable bootstrap and
remove its password. Bootstrap is disabled by default and becomes a no-op once a
super administrator exists.

```powershell
$env:BootstrapAdmin__Enabled = "true"
$env:BootstrapAdmin__Email = "admin@example.com"
$env:BootstrapAdmin__UserName = "superadmin"
$env:BootstrapAdmin__Password = "replace-with-a-strong-unique-password"
```

## Outbox messages

`OutboxMessage` stores an integration event in the identity database as part of
the same transaction as the identity change. The optional background dispatcher
posts signed JSON envelopes to a webhook, uses leases across service instances,
retries with exponential backoff, and dead-letters exhausted events. This prevents
events such as `UserRegistered` from being lost when the database commit succeeds
but the external consumer is temporarily unavailable.

Outbox messages are not application logs. Structured request audit and operational
logs record method, path, status, subject ID, trace ID, dispatch results, and cleanup
counts without logging passwords, OTP values, access tokens, or refresh tokens.

## Passwords, logout, and account deletion

- `POST /api/auth/password/forgot` takes an email and returns a generic `202`
  acknowledgment for eligible, unknown, inactive, and cooldown-limited accounts.
  Delivery failure is logged without changing that acknowledgment. This normalizes
  the response body and status; sensitive public endpoints are rate limited.
- `POST /api/auth/password/reset` takes `challengeId`, `code`, and `newPassword`
  from the recovery flow. The email contains both the challenge ID and OTP.
- `POST /api/auth/password/change` requires a bearer session, `currentPassword`,
  and `newPassword`. Failed current-password checks contribute to account lockout.
- `POST /api/auth/logout` revokes the current session; `POST /api/auth/logout-all`
  revokes every session belonging to the authenticated user. Both return `204`.
- `POST /api/users/me/deletion/request` requires a bearer session and
  `currentPassword`, then emails a deletion OTP and returns its challenge metadata.
- `POST /api/users/me/deletion/confirm` requires the same account's bearer session,
  `challengeId`, and `code`. It returns `204`. The final super administrator cannot
  be deleted; this is checked both when requesting and confirming deletion.

Recovery and deletion reuse `Registration` OTP settings (10-minute expiry, five
attempts, 60-second resend cooldown by default). Codes are purpose-bound, hashed,
single-use, and invalidated by resends. Password reset/change rejects the current
password and the previous five passwords; configure the previous-password window
with `AccountLifecycle:PasswordHistoryCount` (1–24). Only versioned password hashes
are retained, and the history is pruned on each successful password update.

Password reset/change revokes all sessions and refresh tokens, rotates the security
stamp, clears lockout, and invalidates outstanding OTPs, including deletion codes.
Clients must sign in again. A signed-out access token is rejected even before its
JWT expiry; repeating logout with that token returns `401`.

Account deletion initially retains a tombstone ID with a deleted status and anonymized
primary profile. Passwords, password history, external logins, stored user tokens,
claims, roles, and OTPs are removed; devices and sessions are revoked and push
registrations cleared. Scheduled cleanup removes eligible session/device history
and hard-purges the tombstone after `Cleanup:DeletedAccountRetentionDays` (90 days
by default). Processed outbox payloads have an independent retention period.
foodiya.app.Identity and future consumers must handle `UserDeleted` and apply downstream rules.

Email adapters implement both `SendRegistrationOtpAsync` and
`SendAccountOtpAsync`. The Development adapter logs codes only when explicitly
enabled; the SMTP adapter includes the OTP, expiry, and account-operation challenge
reference needed by recovery flows.

## Devices and sessions

All device/session routes require an active bearer session and only operate on
the authenticated account's records:

- `GET /api/users/me/devices?skip=0&take=25` lists device history, verification
  time, current-device status, and whether a Firebase installation is registered.
- `GET /api/users/me/sessions?skip=0&take=25` lists session history and identifies
  active/current sessions. `take` is limited to 100; negative `skip` values are rejected.
- `POST /api/users/me/devices/{deviceId}/revoke` revokes the device, every session
  and refresh token attached to it, and its pending device OTPs. It clears the
  Firebase installation and preserves history. Login with the same revoked installation
  identifier is rejected; this step does not provide device restoration.
- `POST /api/users/me/sessions/{sessionId}/revoke` revokes only the chosen session
  and its refresh tokens. Other sessions and the device remain usable.
- `POST /api/users/me/devices/current/verification/request` sends a device-bound
  email OTP and returns `challengeId` and `expiresAtUtc`.
- `POST /api/users/me/devices/current/verification/confirm` takes `challengeId`
  and `code`, consumes the OTP, and records `VerifiedAtUtc`.
- `PUT /api/users/me/devices/current/push-registration` takes `firebaseInstallationId`.
- `DELETE /api/users/me/devices/current/push-registration` clears that installation.

Device verification records email-confirmed device trust after password login;
it is **not mandatory MFA** and does not gate token issuance. It uses the existing
`DeviceLogin` OTP purpose and `Registration` expiry, attempt-limit, and resend
settings. Codes are bound to the account, normalized email, and hashed device
installation identifier. Another device cannot consume the code.

Revocation and Firebase installation operations return `204`. Unknown or foreign device/session IDs
return `404`. Repeating revocation from another active session is harmless; revoking
the current session/device makes its bearer token unusable, including for retries.

The API stores a Firebase Installation ID (FID) of up to 128 characters; it does
not contact Firebase or send notifications. The same FID can move between the
account's own devices, but cannot be claimed from another account. Firebase
validation and delivery belong to the notification worker. Lists and outbox payloads
never expose FIDs or installation hashes. Applications sharing physical devices
should clear the FID before account switching; logout alone retains it. Device and
account revocation clear it.

## Tests

```powershell
dotnet test --solution foodiya.app.Identity.slnx
```

Security tests use relational SQLite with real Identity password hashing and JWT
issuance. They cover OTP replay/expiry/attempt limits, password reuse and retention,
session revocation, deletion authorization, stale login credentials, cleanup,
SMTP message construction, outbox retry/lease/dead-letter behavior, and the real
rate-limit/localization middleware pipeline. Architecture tests enforce the allowed
layer references.

Generate a local Cobertura coverage report with:

```powershell
dotnet test --solution foodiya.app.Identity.slnx --configuration Release `
  --coverage --coverage-output coverage.cobertura.xml `
  --coverage-output-format cobertura --results-directory artifacts/test-results
```

## Setup and deployment

See [SETUP.md](SETUP.md) for secret generation, Docker Compose startup,
automatic migrations, the persistent SQL Server volume, and local .NET commands.
See [consumer integration](docs/consumer-integration.md) for the JWT contract and
resource-API setup.
