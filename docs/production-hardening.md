# Production hardening

The repository still contains exactly two application projects. This pass fixes
ownership, recipe visibility, production defaults, SQL readiness, request limits,
and CI regression coverage. It does not deploy an Azure environment.

## Access rules

| Resource/action | Allowed callers |
| --- | --- |
| Recipe browse/search | Authenticated callers; PUBLIC, published, active recipes only |
| Draft or inactive recipe | Owner or administrator |
| PRIVATE published recipe and its children | Owner, administrator, or an explicitly authorized recipient |
| UNLISTED published recipe | A caller with its UUID; excluded from general browse/search |
| Recipe and child-resource changes | Owning chef or administrator, subject to endpoint role requirements |
| Chef profile editing | Profile owner or administrator |
| Like creation/deletion | Authenticated acting user only |
| Share creation | Authenticated sender; private sharing requires owner/admin authority |
| Share editing/deletion | Sender or administrator |
| Share messages/participant metadata | Sender, recipient, or administrator |

Deleting a share removes the recipient's access grant. Drafts and inactive
recipes stay restricted even if a share exists. Deleted recipes are unavailable.
Child collections and their pagination counts use the same visibility policy.

Acting and deletion-audit identities come from the validated JWT subject. Existing
user-UUID request fields remain compatible when they match that subject; another
user's UUID is rejected. An omitted/empty acting UUID uses the authenticated user.
Ownership failures return 403; inaccessible private reads return 404.

## Request limits

- Pages: 1 through 10,000; page sizes: 1 through 100. Invalid values return 400.
- Request bodies: at most 1 MiB. Oversized bodies return 413.
- Recipe ingredient, step and category collections: at most 100 elements each.
- Business API: 120 controller requests per minute per authenticated subject by
  default, with an IP fallback for unauthenticated traffic. Excess requests return
  429 and Retry-After. Configure `ApiLimits__PermitLimit` and `ApiLimits__WindowSeconds`.
- Identity retains its separate ingress/global/sensitive-operation policies.

Quotas are per process. Configure gateway-wide protection for a multi-instance
Azure deployment. Recipe detail/list queries use split queries for multiple
sibling collections, and recipe paging includes a unique ID tie-breaker.
Measure production query plans and latency with representative data before scale-up.

## Production configuration

The main Compose file defaults to Production and both API runtime images use a
non-root user. Scalar, Swagger UI and generated OpenAPI documents are exposed
only in Development. The local override is:

```powershell
docker compose -f docker-compose.yml -f docker-compose.Development.yml up -d --build
```

Production needs:

- Separate configured SQL connections. Use encrypted connections and the intended
  production identity/permissions; the local SQL Developer/sa Compose setup is for
  local development, not the production database deployment.
- HTTPS JWT authority and issuer with the configured audience.
- Identity's configured RSA private key and KeyId, independent OTP and refresh
  peppers, and SMTP configuration. Disabled/development email providers are rejected
  outside Development. The `.env.example` includes SMTP setting names.
- Exact HTTPS browser origins in `CorsOrigins__0`, `CorsOrigins__1`, etc. No browser
  origins are allowed by default in Production. The main Compose stack accepts
  `CORS_ORIGIN`; its local override accepts `DEVELOPMENT_CORS_ORIGIN`.
- HTTPS ingress enforced by Azure/the reverse proxy, intended host names through
  `AllowedHosts`, and secrets supplied through the deployment secret store.

Reverse-proxy forwarding is an explicit opt-in, independent of host forwarding
flags. Set trusted peer addresses/networks and the intended hop limit, for example:

```json
{
  "ReverseProxy": {
    "Enabled": true,
    "ForwardLimit": 1,
    "KnownProxies": ["10.0.0.10"],
    "KnownNetworks": []
  }
}
```

Replace that example address with the actual trusted proxy socket peer. For an
Azure/Cloudflare chain, configure the real trusted peers and hop count and restrict
backend ingress. Do not trust all networks or read a client-supplied forwarded IP
directly. Forwarding runs before authentication, auditing and rate limiting.
Enabling forwarding without trusted addresses causes startup failure.

`/health` reports liveness. `/health/ready` checks SQL even when RabbitMQ is disabled;
it returns 503 while SQL is unavailable. Messaging adds its own readiness check
when enabled. Readiness deliberately does not send SMTP email or prove external
TLS/DNS configuration.

## Verification

After a Release build, on a dedicated local SQL Server using Windows authentication:

```powershell
dotnet build API-Template.slnx --configuration Release
./scripts/Test-Integration.ps1
```

For isolated build outputs while other copies are running:

```powershell
dotnet build API-Template.slnx --configuration Release --artifacts-path artifacts/hardening
./scripts/Test-Integration.ps1 -ArtifactsRoot artifacts/hardening
```

The runner requires PowerShell 7.4+, .NET 10, and either sqlcmd or the CI SQL
container. It creates two uniquely named databases and its own loopback API
processes, and removes only those databases/processes afterwards. It refuses
occupied ports. Generated test signing keys and logs stay in ignored artifacts.
Credentials, OTPs and tokens are not printed.

The checks cover valid and forged acting identities, two-chef ownership, draft/
private/unlisted content, child resources, share grants/revocation, pagination,
large bodies/collections, independent rate-limit quotas, real registration/OTP/
login/refresh/logout, concurrent refresh reuse, SQL outages, Production startup,
CORS and rejected unsafe configurations. CI runs them against SQL Server 2022,
including a French locale to exercise decimal validation.

Concurrent refresh reads lock the token before state inspection, so two attempts
produce one rotation and one reuse rejection instead of a lock-upgrade deadlock.
Reuse revokes the compromised Identity session.

## Still required before a production release

- Validate actual Azure HTTPS, proxy peers, DNS, network access, SMTP delivery,
  monitoring, secrets, backups and restore in staging.
- Test RabbitMQ outage/recovery and capacity under representative load.
- Use production seed data and reviewed forward schema upgrades. The single SQL
  baseline remains a development initializer with sample users and recipes.
- Plan overlapping signing-key rotation if uninterrupted token validity is needed.
- Define the revocation SLA: Identity checks active sessions immediately; Foodiya
  accepts existing signed access tokens until expiry. Downstream role/deletion
  event handling and immediate token introspection are not added by this pass.
