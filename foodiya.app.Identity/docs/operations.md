# Operations runbook

Step 8 features are safe-by-default: SMTP, cleanup, and outbox delivery require
explicit configuration, and no production secret belongs in `appsettings.json`.
Use environment variables, a platform secret store, or a managed vault.

## Production secrets

Required identity secrets:

```text
Registration__OtpPepper
Tokens__RefreshTokenPepper
Tokens__RsaPrivateKeyPath=/run/secrets/jwt-private-key.pem
Tokens__KeyId=foodiya-app-identity-2026-09
```

Use an RSA private key of at least 2048 bits and independent, cryptographically
random pepper values of at least 32 characters. Never log or reuse them.

Secret rotation affects active credentials:

- Rotating the RSA private key immediately rejects access tokens signed by the
  previous key. Assign the replacement a new `Tokens__KeyId`; with the current
  single-key template, rotate during a controlled window and rely on the short
  access-token lifetime.
- Rotating `Tokens__RefreshTokenPepper` invalidates every refresh token. Revoke
  active sessions and require users to sign in again.
- Rotating `Registration__OtpPepper` invalidates outstanding OTP challenges.
  Perform it after invalidating current OTPs or accept that users must request a
  new code.

## SMTP email

Configure the production adapter through the secret store:

```text
EmailDelivery__Provider=Smtp
EmailDelivery__Host=smtp.example.com
EmailDelivery__Port=587
EmailDelivery__FromAddress=identity@example.com
EmailDelivery__UserName=...
EmailDelivery__Password=...
EmailDelivery__UseStartTls=true
EmailDelivery__TimeoutSeconds=20
```

`StartTls` is required when enabled; use a server with a valid certificate. The
`Development` provider and `LogDevelopmentCodes` are only for local development.

## Outbox webhook

Enable delivery only after the receiver is ready:

```text
Outbox__Enabled=true
Outbox__Endpoint=https://events.example.com/identity
Outbox__SigningSecret=replace-with-an-independent-random-secret
```

Each POST contains `id`, `type`, `occurredAtUtc`, and the JSON `payload`. It also
contains `Idempotency-Key: <event-id>` and `X-Identity-Signature: sha256=<hex>`,
where the signature is HMAC-SHA256 over the exact UTF-8 request body. Receivers
must verify the signature with constant-time comparison and process the event ID
idempotently.

Rows are claimed with expiring leases, retried with bounded exponential backoff,
and marked `DeadLetteredAtUtc` after `MaxAttempts`. Alert on dead-lettered rows;
after correcting the receiver or payload, an operator may clear the error,
attempt count, dead-letter timestamp, next-attempt timestamp, and lease fields to
requeue a specific reviewed event.

## Cleanup and retention

Enable `Cleanup__Enabled=true` after confirming legal and product retention rules.
The default hourly, bounded job removes:

- sessions expired or revoked beyond 30 days, cascading their refresh tokens;
- OTP challenges expired beyond 7 days;
- processed outbox events older than 30 days;
- registration-attempt audit rows older than 30 days;
- anonymized deleted-account tombstones older than 90 days, including remaining
  device/session/history rows.

Pending and dead-lettered outbox messages are not purged. Rotated refresh-token
history belonging to a usable session is retained for reuse detection.

## Rate limiting and localization

The API applies a global fixed window of 120 requests per minute per authenticated
subject (or source IP for anonymous callers). Registration, login, refresh, and
password endpoints also use a 10-request-per-minute sensitive policy. Rejections
return HTTP `429`, `Retry-After`, code `RATE_LIMIT_EXCEEDED`, and Problem Details.

`Accept-Language: en`, `Accept-Language: fr`, and `Accept-Language: ar` select localized Problem Details
titles. Clients should localize stable machine-readable error codes themselves;
free-form details remain diagnostic English text.

## Logs and OpenTelemetry

Audit logs contain request method/path, response status, subject ID, duration, and
trace ID. They intentionally exclude request bodies and all credentials. Restrict
log access and apply the retention policy required by the deployment.

ASP.NET Core HTTP traces/metrics and bounded identity-operation metrics are
registered with OpenTelemetry. Set `OTEL_EXPORTER_OTLP_ENDPOINT` to activate OTLP
export; leave it unset when no collector exists. Configure OTLP authentication
through the standard `OTEL_EXPORTER_OTLP_HEADERS` environment variable.

## Deployment checklist

1. Apply EF Core migrations before enabling new instances.
2. Supply independent OTP, JWT, refresh-token, outbox, and SMTP secrets.
3. Verify SMTP delivery and webhook signature validation in staging.
4. Enable cleanup only after approving retention periods.
5. Configure an OTLP collector and alerts for HTTP failures, cleanup failures,
   repeated outbox retries, and dead-lettered events.
6. Confirm logs do not leave the approved security boundary.
