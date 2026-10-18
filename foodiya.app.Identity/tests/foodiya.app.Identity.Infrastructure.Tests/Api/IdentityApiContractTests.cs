using System.Collections.Concurrent;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Security.Claims;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using foodiya.app.Identity.Application.Exceptions;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Contracts.Authentication;
using foodiya.app.Identity.Contracts.Registration;
using foodiya.app.Identity.Domain.Enums;
using foodiya.app.Identity.Infrastructure.Authentication;
using foodiya.app.Identity.Infrastructure.Identity;
using Microsoft.IdentityModel.Tokens;

namespace foodiya.app.Identity.Infrastructure.Tests.Api;

[TestClass]
public sealed class IdentityApiContractTests : IAsyncDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly TestContext _testContext;
    private readonly AuditLogCollector _auditLogs;

    public IdentityApiContractTests(TestContext testContext)
    {
        _testContext = testContext;
        _auditLogs = new AuditLogCollector();
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureLogging(logging =>
            {
                logging.ClearProviders();
                logging.SetMinimumLevel(LogLevel.Information);
                logging.AddProvider(_auditLogs);
            });
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IIdentityAuthenticationService>();
                services.AddSingleton<IIdentityAuthenticationService, LockedAuthenticationService>();
                services.AddDataProtection().UseEphemeralDataProtectionProvider();
            });
        });
        _client = _factory.CreateClient();
    }

    [TestMethod]
    public async Task CurrentUser_WithoutBearerToken_ReturnsProblemDetailsChallenge()
    {
        using var response = await _client.GetAsync("/api/users/me", _testContext.CancellationToken);
        using var document = JsonDocument.Parse(await response.Content
            .ReadAsStreamAsync(_testContext.CancellationToken));

        AssertStandardProblem(response, document, HttpStatusCode.Unauthorized, "AUTHENTICATION_REQUIRED");
        Assert.AreEqual("Bearer", response.Headers.WwwAuthenticate.Single().Scheme);
        Assert.AreEqual("Authentication required", document.RootElement.GetProperty("title").GetString());
    }

    [TestMethod]
    public async Task CurrentUser_WithArabicLanguage_ReturnsArabicProblemDetailsChallenge()
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, "/api/users/me");
        request.Headers.AcceptLanguage.ParseAdd("ar");

        using var response = await _client.SendAsync(request, _testContext.CancellationToken);
        using var document = JsonDocument.Parse(await response.Content
            .ReadAsStreamAsync(_testContext.CancellationToken));

        AssertStandardProblem(response, document, HttpStatusCode.Unauthorized, "AUTHENTICATION_REQUIRED");
        Assert.AreEqual("المصادقة مطلوبة", document.RootElement.GetProperty("title").GetString());
        Assert.AreEqual(
            "يلزم رمز Bearer صالح مرتبط بجلسة نشطة.",
            document.RootElement.GetProperty("detail").GetString());
    }

    [TestMethod]
    public async Task Login_WhenAccountIsLocked_ReturnsLocalizedLockedContract()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new LoginRequest
            {
                Login = "locked-user",
                Password = "Valid!Password123",
                DeviceInstallationId = "locked-device"
            })
        };
        request.Headers.AcceptLanguage.ParseAdd("fr");

        using var response = await _client.SendAsync(request, _testContext.CancellationToken);
        using var document = JsonDocument.Parse(await response.Content
            .ReadAsStreamAsync(_testContext.CancellationToken));

        AssertStandardProblem(response, document, (HttpStatusCode)423, "ACCOUNT_LOCKED");
        Assert.AreEqual("Compte verrouillé", document.RootElement.GetProperty("title").GetString());
        Assert.IsNotNull(response.Headers.RetryAfter?.Delta);
        Assert.IsGreaterThan(TimeSpan.Zero, response.Headers.RetryAfter.Delta.Value);
    }

    [TestMethod]
    public async Task Login_WithInvalidFields_ReturnsStandardValidationProblem()
    {
        using var response = await _client.PostAsJsonAsync(
            "/api/auth/login",
            new { login = "x", password = "short", deviceInstallationId = "id" },
            _testContext.CancellationToken);
        using var document = JsonDocument.Parse(await response.Content
            .ReadAsStreamAsync(_testContext.CancellationToken));

        AssertStandardProblem(response, document, HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        Assert.AreEqual(JsonValueKind.Object, document.RootElement.GetProperty("errors").ValueKind);
    }

    [TestMethod]
    public async Task Login_WithInvalidFieldsAndArabicLanguage_LocalizesValidationProblem()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = JsonContent.Create(new { login = "x", password = "short", deviceInstallationId = "id" })
        };
        request.Headers.AcceptLanguage.ParseAdd("ar");

        using var response = await _client.SendAsync(request, _testContext.CancellationToken);
        using var document = JsonDocument.Parse(await response.Content
            .ReadAsStreamAsync(_testContext.CancellationToken));

        AssertStandardProblem(response, document, HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        Assert.AreEqual("طلب غير صالح", document.RootElement.GetProperty("title").GetString());
        Assert.AreEqual(
            "حقل واحد أو أكثر في الطلب غير صالح.",
            document.RootElement.GetProperty("detail").GetString());
        foreach (var field in document.RootElement.GetProperty("errors").EnumerateObject())
        {
            Assert.AreEqual("قيمة هذا الحقل غير صالحة.", field.Value[0].GetString());
        }
    }

    [TestMethod]
    public async Task Register_WithArabicHeader_LocalizesValidationProblem()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/register")
        {
            Content = JsonContent.Create(new RegisterRequest
            {
                UserName = "x",
                Email = "invalid",
                Password = "short",
                FirstName = "",
                LastName = "",
                PreferredLanguage = "ar"
            })
        };
        request.Headers.AcceptLanguage.ParseAdd("ar");

        using var response = await _client.SendAsync(request, _testContext.CancellationToken);
        using var document = JsonDocument.Parse(await response.Content
            .ReadAsStreamAsync(_testContext.CancellationToken));

        AssertStandardProblem(response, document, HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        Assert.AreEqual("طلب غير صالح", document.RootElement.GetProperty("title").GetString());
        Assert.AreEqual(
            "حقل واحد أو أكثر في الطلب غير صالح.",
            document.RootElement.GetProperty("detail").GetString());
        foreach (var field in document.RootElement.GetProperty("errors").EnumerateObject())
        {
            Assert.AreEqual("قيمة هذا الحقل غير صالحة.", field.Value[0].GetString());
        }
    }

    [TestMethod]
    public async Task Register_WithArabicBodyPreferenceAndNoLanguageHeader_UsesDefaultEnglishValidationProblem()
    {
        using var response = await _client.PostAsJsonAsync(
            "/api/auth/register",
            new RegisterRequest
            {
                UserName = "x",
                Email = "invalid",
                Password = "short",
                FirstName = "",
                LastName = "",
                PreferredLanguage = "ar"
            },
            _testContext.CancellationToken);
        using var document = JsonDocument.Parse(await response.Content
            .ReadAsStreamAsync(_testContext.CancellationToken));

        AssertStandardProblem(response, document, HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        Assert.AreEqual("Invalid request", document.RootElement.GetProperty("title").GetString());
        Assert.AreEqual(
            "One or more request fields are invalid.",
            document.RootElement.GetProperty("detail").GetString());
    }

    [TestMethod]
    public async Task Register_WithFrenchHeaderAndArabicBodyPreference_UsesFrenchValidationProblem()
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/register")
        {
            Content = JsonContent.Create(new RegisterRequest
            {
                UserName = "x",
                Email = "invalid",
                Password = "short",
                FirstName = "",
                LastName = "",
                PreferredLanguage = "ar"
            })
        };
        request.Headers.AcceptLanguage.ParseAdd("fr");

        using var response = await _client.SendAsync(request, _testContext.CancellationToken);
        using var document = JsonDocument.Parse(await response.Content
            .ReadAsStreamAsync(_testContext.CancellationToken));

        AssertStandardProblem(response, document, HttpStatusCode.BadRequest, "VALIDATION_FAILED");
        Assert.AreEqual("Requête invalide", document.RootElement.GetProperty("title").GetString());
        Assert.AreEqual(
            "Un ou plusieurs champs de la requête sont invalides.",
            document.RootElement.GetProperty("detail").GetString());
    }

    [TestMethod]
    public async Task Login_WithMalformedJson_ReturnsStandardMalformedRequestProblem()
    {
        using var content = new StringContent("{", Encoding.UTF8, "application/json");
        using var response = await _client.PostAsync(
            "/api/auth/login", content, _testContext.CancellationToken);
        using var document = JsonDocument.Parse(await response.Content
            .ReadAsStreamAsync(_testContext.CancellationToken));

        AssertStandardProblem(response, document, HttpStatusCode.BadRequest, "MALFORMED_REQUEST");
    }

    [TestMethod]
    public async Task UnknownRoute_ReturnsStandardNotFoundProblem()
    {
        using var response = await _client.GetAsync("/api/not-a-real-route", _testContext.CancellationToken);
        using var document = JsonDocument.Parse(await response.Content
            .ReadAsStreamAsync(_testContext.CancellationToken));

        AssertStandardProblem(response, document, HttpStatusCode.NotFound, "RESOURCE_NOT_FOUND");
    }

    [TestMethod]
    public async Task Login_WithUnsupportedMethod_ReturnsStandardMethodNotAllowedProblem()
    {
        using var response = await _client.GetAsync("/api/auth/login", _testContext.CancellationToken);
        using var document = JsonDocument.Parse(await response.Content
            .ReadAsStreamAsync(_testContext.CancellationToken));

        AssertStandardProblem(response, document, HttpStatusCode.MethodNotAllowed, "METHOD_NOT_ALLOWED");
    }

    [TestMethod]
    public async Task GoogleLogin_WhenUnexpectedExceptionOccurs_ReturnsStandardInternalErrorProblem()
    {
        using var response = await _client.PostAsJsonAsync(
            "/api/auth/google",
            new GoogleSignInRequest
            {
                IdToken = "test-google-id-token",
                DeviceInstallationId = "test-device-installation"
            },
            _testContext.CancellationToken);
        using var document = JsonDocument.Parse(await response.Content
            .ReadAsStreamAsync(_testContext.CancellationToken));

        AssertStandardProblem(
            response,
            document,
            HttpStatusCode.InternalServerError,
            "INTERNAL_SERVER_ERROR");
        Assert.DoesNotContain("NotSupportedException", document.RootElement.GetProperty("detail").GetString()!);
    }

    [TestMethod]
    public async Task GoogleLogin_WhenUnexpectedExceptionOccurs_AuditRecordsHandledStatus()
    {
        using var response = await _client.PostAsJsonAsync(
            "/api/auth/google",
            new GoogleSignInRequest
            {
                IdToken = "test-google-id-token",
                DeviceInstallationId = "test-device-installation"
            },
            _testContext.CancellationToken);

        Assert.AreEqual(HttpStatusCode.InternalServerError, response.StatusCode);
        var auditMessage = _auditLogs.Messages.SingleOrDefault(message =>
            message.Contains("POST /api/auth/google", StringComparison.Ordinal));
        Assert.IsNotNull(auditMessage);
        Assert.Contains("returned 500", auditMessage);
    }

    [TestMethod]
    public async Task OpenApi_AllOperationsDefineAcceptLanguageHeader()
    {
        using var response = await _client.GetAsync("/openapi/v1.json", _testContext.CancellationToken);
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content
            .ReadAsStreamAsync(_testContext.CancellationToken));

        var operationCount = 0;
        foreach (var path in document.RootElement.GetProperty("paths").EnumerateObject())
        {
            foreach (var operation in path.Value.EnumerateObject()
                         .Where(candidate => IsHttpOperation(candidate.Name)))
            {
                operationCount++;
                var languageParameter = operation.Value.GetProperty("parameters")
                    .EnumerateArray()
                    .Single(parameter =>
                        parameter.GetProperty("name").GetString() == "Accept-Language"
                        && parameter.GetProperty("in").GetString() == "header");

                Assert.IsFalse(
                    languageParameter.TryGetProperty("required", out var required) && required.GetBoolean(),
                    $"{operation.Name.ToUpperInvariant()} {path.Name} should make Accept-Language optional.");
                Assert.AreEqual("en", languageParameter.GetProperty("schema").GetProperty("default").GetString());
                var supportedLanguages = languageParameter.GetProperty("schema").GetProperty("enum")
                    .EnumerateArray()
                    .Select(value => value.GetString())
                    .ToArray();
                Assert.HasCount(3, supportedLanguages);
                Assert.Contains("en", supportedLanguages);
                Assert.Contains("fr", supportedLanguages);
                Assert.Contains("ar", supportedLanguages);
            }
        }

        Assert.IsGreaterThan(0, operationCount);
    }

    [TestMethod]
    public async Task OpenApi_DefinesBearerSecurityForProtectedOperations()
    {
        using var response = await _client.GetAsync("/openapi/v1.json", _testContext.CancellationToken);
        response.EnsureSuccessStatusCode();
        using var document = JsonDocument.Parse(await response.Content
            .ReadAsStreamAsync(_testContext.CancellationToken));

        var bearerScheme = document.RootElement
            .GetProperty("components")
            .GetProperty("securitySchemes")
            .GetProperty("Bearer");
        Assert.AreEqual("http", bearerScheme.GetProperty("type").GetString());
        Assert.AreEqual("bearer", bearerScheme.GetProperty("scheme").GetString());
        Assert.AreEqual("JWT", bearerScheme.GetProperty("bearerFormat").GetString());

        var protectedOperation = document.RootElement
            .GetProperty("paths")
            .GetProperty("/api/users/me")
            .GetProperty("get");
        var protectedSecurity = protectedOperation.GetProperty("security")[0];
        Assert.IsTrue(protectedSecurity.TryGetProperty("Bearer", out _));

        var publicOperation = document.RootElement
            .GetProperty("paths")
            .GetProperty("/api/auth/login")
            .GetProperty("post");
        Assert.IsFalse(publicOperation.TryGetProperty("security", out _));
    }

    [TestMethod]
    public async Task DiscoveryAndJwks_ValidateIssuedRs256AccessToken()
    {
        using var discoveryResponse = await _client.GetAsync(
            "/.well-known/openid-configuration",
            _testContext.CancellationToken);
        discoveryResponse.EnsureSuccessStatusCode();
        using var discovery = JsonDocument.Parse(await discoveryResponse.Content
            .ReadAsStreamAsync(_testContext.CancellationToken));

        var issuer = discovery.RootElement.GetProperty("issuer").GetString();
        var jwksUri = discovery.RootElement.GetProperty("jwks_uri").GetString();
        Assert.IsFalse(discovery.RootElement.TryGetProperty("jwksUri", out _));
        Assert.IsFalse(discovery.RootElement.TryGetProperty("authorization_endpoint", out _));
        Assert.IsNotNull(issuer);
        Assert.IsNotNull(jwksUri);

        using var jwksResponse = await _client.GetAsync(
            new Uri(jwksUri).PathAndQuery,
            _testContext.CancellationToken);
        jwksResponse.EnsureSuccessStatusCode();
        var jwks = new JsonWebKeySet(await jwksResponse.Content
            .ReadAsStringAsync(_testContext.CancellationToken));

        var token = _factory.Services.GetRequiredService<IAccessTokenService>().Create(
            new ApplicationUser
            {
                Id = Guid.NewGuid(),
                UserName = "discovery-test",
                Email = "discovery@example.test",
                FirstName = "Discovery",
                LastName = "Test",
                SecurityStamp = "discovery-security-stamp",
                Status = AccountStatus.Active
            },
            ["USER"],
            Guid.NewGuid(),
            Guid.NewGuid(),
            DateTimeOffset.UtcNow.AddMinutes(5)).Token;

        var principal = new JwtSecurityTokenHandler { MapInboundClaims = false }.ValidateToken(
            token,
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = "foodiya.app.Identity.Clients",
                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = jwks.Keys,
                ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromSeconds(30)
            },
            out _);

        Assert.IsNotNull(principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value);
    }

    private static bool IsHttpOperation(string propertyName) => propertyName is
        "get" or "put" or "post" or "delete" or "options" or "head" or "patch" or "trace";

    private static void AssertStandardProblem(
        HttpResponseMessage response,
        JsonDocument document,
        HttpStatusCode expectedStatus,
        string expectedCode)
    {
        var problem = document.RootElement;
        Assert.AreEqual(expectedStatus, response.StatusCode);
        Assert.AreEqual("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        Assert.AreEqual((int)expectedStatus, problem.GetProperty("status").GetInt32());
        Assert.AreEqual(expectedCode, problem.GetProperty("code").GetString());
        Assert.IsFalse(string.IsNullOrWhiteSpace(problem.GetProperty("type").GetString()));
        Assert.IsFalse(string.IsNullOrWhiteSpace(problem.GetProperty("title").GetString()));
        Assert.IsFalse(string.IsNullOrWhiteSpace(problem.GetProperty("detail").GetString()));
        Assert.IsFalse(string.IsNullOrWhiteSpace(problem.GetProperty("instance").GetString()));
        Assert.IsFalse(string.IsNullOrWhiteSpace(problem.GetProperty("traceId").GetString()));
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    private sealed class LockedAuthenticationService : IIdentityAuthenticationService
    {
        public Task<AuthenticationTokenResponse> LoginAsync(LoginRequest request, string? ipAddress,
            CancellationToken cancellationToken) => throw new IdentityLockedException(
                "ACCOUNT_LOCKED",
                "The account is temporarily locked.",
                DateTimeOffset.UtcNow.AddMinutes(1));

        public Task<AuthenticationTokenResponse> SignInWithGoogleAsync(GoogleSignInRequest request, string? ipAddress,
            CancellationToken cancellationToken) => throw new NotSupportedException();

        public Task<AuthenticationTokenResponse> RefreshAsync(RefreshAccessTokenRequest request, string? ipAddress,
            CancellationToken cancellationToken) => throw new NotSupportedException();
    }

    private sealed class AuditLogCollector : ILoggerProvider
    {
        private const string AuditCategory =
            "foodiya.app.Identity.Api.Middleware.IdentityAuditMiddleware";
        private readonly ConcurrentQueue<string> _messages = new();

        public IReadOnlyCollection<string> Messages => _messages.ToArray();

        public ILogger CreateLogger(string categoryName) =>
            new AuditLogger(categoryName, _messages);

        public void Dispose()
        {
        }

        private sealed class AuditLogger(
            string categoryName,
            ConcurrentQueue<string> messages) : ILogger
        {
            public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

            public bool IsEnabled(LogLevel logLevel) =>
                categoryName == AuditCategory && logLevel >= LogLevel.Information;

            public void Log<TState>(
                LogLevel logLevel,
                EventId eventId,
                TState state,
                Exception? exception,
                Func<TState, Exception?, string> formatter)
            {
                if (IsEnabled(logLevel))
                {
                    messages.Enqueue(formatter(state, exception));
                }
            }
        }
    }
}
