# Consumer API integration

The generated Identity service owns credentials and authentication workflows.
A product API remains responsible for its business profile and authorization at
the resource level.

## JWT contract

Configure the consumer API to validate:

- Identity as its `Tokens:Authority`, using
  `/.well-known/openid-configuration` and the advertised `jwks_uri`;
- the exact Identity `Tokens:Audience`;
- RS256 signature and lifetime, with only a small clock-skew allowance;
- `name` as the name claim and `role` as the role claim.

Consumer APIs never receive private signing material. They cache Identity's
public JWKS and refresh it when a token references a new key ID.

Use the GUID `sub` claim as the stable external identity ID. Do not use email or
user name as a database key because either can change. A common product model is
to store `sub` in a unique `IdentityUserId` or `Uuid` column and create the local
profile lazily on the first authenticated request. Keep product data in the
consumer database; keep passwords, OTPs, refresh tokens, and sessions in Identity.

## Client flow

1. Register and confirm the OTP through Identity.
2. Sign in through Identity and receive the access/refresh-token response.
3. Send the access token to product APIs in the HTTP header
   `Authorization: Bearer <access-token>`.
4. Send refresh tokens only to Identity's refresh endpoint. Never send tokens in
   query strings or application logs.

Access tokens are intentionally short lived. Logout, password changes, role
changes, and account deletion immediately invalidate sessions inside Identity;
an independently validating resource API can accept an already issued access
token until its short expiry. Add event-driven denylisting or token introspection
only when a product requires immediate cross-service revocation.

## Error response contract

Identity returns non-success responses as `application/problem+json` with one
stable shape:

```json
{
  "type": "https://tools.ietf.org/html/rfc9110#section-15.5.1",
  "title": "Invalid request",
  "status": 400,
  "detail": "The request could not be processed.",
  "instance": "/api/auth/register",
  "code": "BAD_REQUEST",
  "traceId": "00-..."
}
```

The frontend can display `detail` directly and use `code` only when it needs
special behavior:

```javascript
const problem = await response.json();
toast.error(problem.detail ?? problem.title ?? "Something went wrong");
```

Validation failures use the same fields and add an `errors` object containing
field-specific messages. Common stable codes include `VALIDATION_FAILED`,
`MALFORMED_REQUEST`, `AUTHENTICATION_REQUIRED`, `AUTHORIZATION_FORBIDDEN`,
`RESOURCE_NOT_FOUND`, `METHOD_NOT_ALLOWED`, `RATE_LIMIT_EXCEEDED`, and
`INTERNAL_SERVER_ERROR`. Send `Accept-Language: en`, `fr`, or `ar` to receive
localized human-readable `title`, `detail`, and validation messages; application
logic should depend on `code`, not translated text. Include `traceId` when
reporting a server error.

Set `preferredLanguage` to `en`, `fr`, or `ar` during registration. Regional
values such as `fr-FR` and `ar-MA` are normalized to their supported base
language. OTP, welcome, device-verification, deletion-request, and
deletion-confirmation emails use the stored preference. Arabic HTML emails are
rendered with `lang="ar"` and right-to-left direction. On the registration
endpoint, `preferredLanguage` also selects localized validation and service
errors when `Accept-Language` is absent; an explicit `Accept-Language` header
always takes priority.

## Production configuration

Run the Identity database migration as a deployment step and store signing keys,
OTP peppers, refresh-token peppers, SMTP credentials, and bootstrap credentials
in the platform secret store. Do not commit production values. Use HTTPS between
clients and both APIs, restrict CORS to known clients, and configure a production
email provider before enabling public registration or recovery.

Docker is optional. The service can run as a normal `dotnet` process, Windows
service, Azure App Service, container, or another .NET-compatible hosting target.
