# Foodiya RabbitMQ

RabbitMQ runs as a shared broker. Identity publishes committed SQL outbox events;
Foodiya.API consumes them in a background worker. Messaging code is included in
each API's `Messaging/` folder and uses RabbitMQ.Client
7.2.2. The broker image is pinned to `rabbitmq:4.3.6-management`.

## Start the local broker

From the repository root:

```powershell
pwsh ./deployment/rabbitmq/scripts/Start-Local.ps1
```

The script creates a random password in this directory's ignored `.env` file
without replacing existing credentials. The default local ports are:

| Endpoint | Address |
| --- | --- |
| AMQP | `localhost:5673` |
| Management UI | `http://localhost:15673` |
| Docker network | `foodiya-messaging`, host `rabbitmq`, port `5672` |

The host ports avoid an existing local broker on 5672/15672. They can be changed
with `RabbitMq__Port` and `RABBITMQ_MANAGEMENT_PORT` in `.env`. Management login
uses `RabbitMq__UserName` and `RabbitMq__Password` from that file. RabbitMQ data
persists in `foodiya_rabbitmq_data`. Changing the `.env` password after the volume
is initialized does not change the broker user's password; rotate it in RabbitMQ
and update client secrets together.

## Enable Identity publishing

The existing webhook is the default transport. To opt into RabbitMQ with Docker:

```powershell
Set-Location ./foodiya.app.Identity.Api
docker compose -f docker-compose.yml -f docker-compose.rabbitmq.yml up -d --build
```

This requires the existing Identity `.env`, signing key, and email provider setup.
The override loads RabbitMQ credentials, enables `Outbox:Transport=RabbitMq`,
joins the broker network, and exposes Identity on port 8081. Set `IDENTITY_ISSUER`
to the public Identity URL used by clients. Apply the migrations before starting
the API as in the existing Compose workflow.

For a local `dotnet run` process, load the RabbitMq-prefixed variables from the
broker `.env` into its environment and set:

```powershell
$env:Outbox__Enabled = 'true'
$env:Outbox__Transport = 'RabbitMq'
$env:RabbitMq__HostName = 'localhost'
# RabbitMq__Port=5673 and credentials come from deployment/rabbitmq/.env.
```

Only one outbox transport is selected per deployment. The webhook keeps its
original envelope and HMAC signature. RabbitMQ publishes persistent messages,
requires publisher confirms, and uses mandatory routing. Missing bindings,
broker failures, or rejected publishes keep events pending for an outbox retry.
Provision subscriber queues before enabling the publisher: exchanges do not
retain events for queues that have not yet been declared.

## Enable the Foodiya profile consumer

Apply `../../foodrecipe.SqlServer/Migration_IdentityInboxMessage.sql` to the Foodiya
database, followed by `../../foodrecipe.SqlServer/Migration_AppUserEmail256.sql`,
then `../../foodrecipe.SqlServer/Migration_IdentityProfileUnicodeNames.sql`, using
the existing SQL deployment process. These migrations are idempotent. The
email migration uses `nvarchar(256)`, matching Identity's Unicode email storage;
profile names use `nvarchar(100)`. The changes preserve email uniqueness and
existing profile data. The broker
and consumer do not run database schema updates at startup.

From the repository root, with the existing Foodiya Compose configuration:

```powershell
docker compose -f docker-compose.yml -f docker-compose.rabbitmq.yml up -d --build
```

The override enables `IdentityEvents:Enabled=true` and loads broker credentials.
The main API requires its SQL connection plus Identity discovery configuration:
`Authentication:Jwt:Authority`, `Issuer`, and `Audience`. It obtains public signing
keys through discovery/JWKS and accepts RS256. It no longer requires a shared
signing secret. `Issuer` and `Audience` must match Identity's `Tokens` settings.
The API must reach both the discovery URL and the `jwks_uri` it advertises.
Docker uses `IDENTITY_AUTHORITY`, `IDENTITY_ISSUER`, and `IDENTITY_AUDIENCE`;
the defaults target `https://identity.foodiya.app` and `foodiya.app.Identity.Clients`.
HTTPS metadata is required outside Development.

For host development, Identity's launch profile uses `http://localhost:5000`.
The updated main API local example uses that same authority and issuer. For a
different port or container setup, configure a matching issuer whose discovery
and JWKS addresses are reachable by the consuming API. Environment variables
override the main API's optional `appsettings.Local.json`.

For local `dotnet run`, set the broker environment variables and
`IdentityEvents__Enabled=true`. `/health/ready` returns 503 while the enabled
consumer or its required database/schema is unavailable; `/health` remains a
liveness endpoint. The consumer verifies its required tables and the 256-character
email and Unicode-name columns before consuming. A database outage pauses consumption and leaves
valid events queued until the dependency recovers.

The worker declares a durable topic exchange `foodiya.identity.events`, a quorum
queue `foodiya.profiles.identity` bound to `identity.#`, and a dead-letter exchange
and queue. Start the consumer before the publisher so the queue bindings exist.
First run the broker, then the SQL inbox migration and Foodiya consumer, then
enable Identity's RabbitMQ outbox.

## Event contract and processing

```json
{
  "id": "aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeeeee",
  "type": "UserRegistered",
  "occurredAtUtc": "2026-10-11T00:00:00Z",
  "schemaVersion": 1,
  "source": "foodiya.app.Identity",
  "payload": {
    "Id": "11111111-2222-3333-4444-555555555555",
    "Email": "user@example.test",
    "UserName": "example",
    "FirstName": "Example",
    "LastName": "User"
  }
}
```

The envelope `id` identifies the event; payload `Id` identifies the user. Payload
properties are read case-insensitively. Passwords, OTPs, and tokens are excluded.
Older registration payloads containing only `Id` and `Email` are supported.

| Event | Routing key |
| --- | --- |
| UserRegistered | `identity.user.registered.v1` |
| UserRolesChanged | `identity.user.roles.changed.v1` |
| UserDeleted | `identity.user.deleted.v1` |
| Other current Identity events | `identity.<lowercase-event-type>.v1` |

This first consumer creates profiles on `UserRegistered`; other event types are
acknowledged without modifying profiles. Additional services should have their
own queues and event handlers. Changing roles or deleting business data through
events requires those handlers to be added deliberately.

Profiles use the Identity subject as `AppUser.Uuid`. The consumer writes the
profile and `IdentityInboxMessage` in one serializable database transaction and
acknowledges only after commit. Replaying the same event or a second registration
event for the same subject cannot create another profile. Existing profile edits,
roles, and deletion state are preserved. Email conflicts with another subject are
dead-lettered for manual resolution rather than silently linking accounts.
Authenticated requests use the same profile provisioner: they reject conflicting
emails with 409 and inactive profiles with 403. Neither path changes an existing
subject or reactivates a profile. Active profiles may refresh their role from the
authenticated token. Supported email length is 256 characters in both services.
New Identity tokens carry `given_name` and `family_name`, so HTTP provisioning
preserves compound names exactly as registration events do. Older tokens retain
the existing full-name fallback.

Delivery is at least once. Publisher confirmation means broker acceptance, not
completion of business processing. The outbox ID becomes AMQP `MessageId` and
remains unchanged on retries. Keep inbox IDs while their source events can still
be replayed; deleting them early removes the duplicate-processing protection.
Outbox finalization also checks the lease owner through EF concurrency handling.
A worker whose lease was replaced cannot overwrite the new owner's state. The
`FenceOutboxLeaseOwnership` Identity migration records this mapping change and
has no database column changes; include it in the normal Identity migration flow.

The consumer uses prefetch 1 and a fresh dependency-injection scope per message.
Individual handler failures, while the database remains available, wait
`IdentityEvents:RetryDelaySeconds` before requeueing;
after `MaxAttempts` they go to `foodiya.profiles.identity.dead`. Invalid JSON,
unsupported schemas, and invalid registration payloads go directly there.
Quorum queues enable at-least-once dead-letter transfer with `reject-publish`
overflow. On RabbitMQ 4.3 the worker uses `basic.reject`, since `basic.nack`
does not increment the broker's failed-delivery counter.

## Verification and operations

For HTTP authentication and business API checks with local SQL Server, run
`./scripts/Test-LocalApis.ps1` from the repository root after starting both APIs.
The two-project template does not include separate broker or SQL test projects.
Verify RabbitMQ delivery in your deployment by registering an account, checking
Identity's outbox, and confirming its profile is created in Foodiya's database.

The main API validates access tokens locally until expiry. Identity's immediate
session revocation checks apply to Identity endpoints; this increment does not
add downstream introspection or role/deletion event handling.

Monitor ready/unacknowledged/dead-letter queue counts in the management UI and
pending/retrying/dead-lettered `OutboxMessages` in Identity's database. Identity's
OpenTelemetry tracing includes the messaging activity source; `traceparent` is
carried to consumers. Logs contain event IDs and outcomes, not message bodies.
Investigate dead-lettered events before replaying them and preserve their IDs.

For production, configure validated TLS (`RabbitMq:UseTls=true`, port 5671 and
the correct certificate server name), separate publisher/consumer accounts,
restricted vhost permissions, private management access, and a monitored broker
cluster. The single local node exercises quorum queues but supplies no redundancy.

References used through Context7:
- [RabbitMQ .NET client guide](https://www.rabbitmq.com/client-libraries/dotnet-api-guide)
- [Publisher confirms and consumer acknowledgements](https://www.rabbitmq.com/docs/confirms)
- [Quorum queues and dead lettering](https://www.rabbitmq.com/docs/quorum-queues)
