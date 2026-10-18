using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Contracts.Authentication;
using foodiya.app.Identity.Infrastructure.Authentication;
using foodiya.app.Identity.Infrastructure.Authorization;
using foodiya.app.Identity.Infrastructure.Options;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Security.Claims;

namespace foodiya.app.Identity.Infrastructure.Tests.Api;

[TestClass]
public sealed class OperationalHardeningApiTests : IAsyncDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private readonly TestContext _testContext;

    public OperationalHardeningApiTests(TestContext testContext)
    {
        _testContext = testContext;
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IIdentityAuthenticationService>();
                services.AddSingleton<IIdentityAuthenticationService, StubAuthenticationService>();
                services.AddDataProtection().UseEphemeralDataProtectionProvider();
            });
        });
        _client = _factory.CreateClient();
    }

    [TestMethod]
    public async Task IngressLimit_RejectsBeforeActiveSessionValidation()
    {
        var validator = new CountingSessionValidator();
        await using var factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureAppConfiguration((_, configuration) => configuration.AddInMemoryCollection(
                new Dictionary<string, string?> { ["RateLimits:Ingress:PermitLimit"] = "2" }));
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IAccessSessionValidator>();
                services.AddSingleton<IAccessSessionValidator>(validator);
            });
        });
        using var client = factory.CreateClient();
        var options = factory.Services.GetRequiredService<IOptions<AuthenticationTokenOptions>>().Value;
        var key = factory.Services.GetRequiredService<IRsaSigningKeyProvider>().GetSigningKey();
        var token = new JwtSecurityTokenHandler().CreateEncodedJwt(new SecurityTokenDescriptor
        {
            Issuer = options.Issuer, Audience = options.Audience, Expires = DateTime.UtcNow.AddMinutes(5),
            Subject = new ClaimsIdentity([new Claim("sub", Guid.NewGuid().ToString())]),
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.RsaSha256)
        });
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        for (var index = 0; index < 2; index++)
        {
            using var response = await client.GetAsync("/health", _testContext.CancellationToken);
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        }
        Assert.AreEqual(2, validator.Calls);
        using var rejected = await client.GetAsync("/health", _testContext.CancellationToken);
        Assert.AreEqual(HttpStatusCode.TooManyRequests, rejected.StatusCode);
        Assert.IsNotNull(rejected.Headers.RetryAfter);
        Assert.AreEqual(2, validator.Calls, "A request over the ingress limit must not invoke session/database validation.");
    }

    private sealed class CountingSessionValidator : IAccessSessionValidator
    {
        public int Calls { get; private set; }
        public Task<ValidatedAccessSession?> ValidateAsync(ClaimsPrincipal principal, CancellationToken cancellationToken)
        {
            Calls++;
            return Task.FromResult<ValidatedAccessSession?>(null);
        }
    }

    [TestMethod]
    public async Task SensitiveEndpoint_EleventhRequest_ReturnsLocalizedRateLimitProblem()
    {
        for (var requestNumber = 1; requestNumber <= 10; requestNumber++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
            {
                Content = CreateLoginContent()
            };
            request.Headers.AcceptLanguage.ParseAdd("fr");
            using var response = await _client.SendAsync(request, _testContext.CancellationToken);
            Assert.AreNotEqual(HttpStatusCode.TooManyRequests, response.StatusCode,
                $"Request {requestNumber} was limited before the configured threshold.");
        }

        using var limitedRequest = new HttpRequestMessage(HttpMethod.Post, "/api/auth/login")
        {
            Content = CreateLoginContent()
        };
        limitedRequest.Headers.AcceptLanguage.ParseAdd("fr");
        using var limitedResponse = await _client.SendAsync(limitedRequest, _testContext.CancellationToken);
        using var document = JsonDocument.Parse(await limitedResponse.Content
            .ReadAsStreamAsync(_testContext.CancellationToken));

        Assert.AreEqual(HttpStatusCode.TooManyRequests, limitedResponse.StatusCode);
        Assert.AreEqual("application/problem+json", limitedResponse.Content.Headers.ContentType?.MediaType);
        Assert.IsFalse(string.IsNullOrWhiteSpace(document.RootElement.GetProperty("type").GetString()));
        Assert.AreEqual("Trop de requêtes", document.RootElement.GetProperty("title").GetString());
        Assert.IsFalse(string.IsNullOrWhiteSpace(document.RootElement.GetProperty("detail").GetString()));
        Assert.AreEqual("/api/auth/login", document.RootElement.GetProperty("instance").GetString());
        Assert.AreEqual("RATE_LIMIT_EXCEEDED", document.RootElement.GetProperty("code").GetString());
        Assert.IsFalse(string.IsNullOrWhiteSpace(document.RootElement.GetProperty("traceId").GetString()));
        Assert.IsNotNull(limitedResponse.Headers.RetryAfter?.Delta);
        Assert.IsGreaterThan(TimeSpan.Zero, limitedResponse.Headers.RetryAfter.Delta.Value);
    }

    public async ValueTask DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        GC.SuppressFinalize(this);
    }

    private static JsonContent CreateLoginContent() => JsonContent.Create(new LoginRequest
    {
        Login = "rate-limit-user",
        Password = "Valid!Password123",
        DeviceInstallationId = "rate-limit-device"
    });

    private sealed class StubAuthenticationService : IIdentityAuthenticationService
    {
        public Task<AuthenticationTokenResponse> LoginAsync(LoginRequest request, string? ipAddress,
            CancellationToken cancellationToken) => Task.FromResult(CreateResponse(cancellationToken));

        public Task<AuthenticationTokenResponse> SignInWithGoogleAsync(GoogleSignInRequest request, string? ipAddress,
            CancellationToken cancellationToken) => Task.FromResult(CreateResponse(cancellationToken));

        public Task<AuthenticationTokenResponse> RefreshAsync(RefreshAccessTokenRequest request, string? ipAddress,
            CancellationToken cancellationToken) => Task.FromResult(CreateResponse(cancellationToken));

        private static AuthenticationTokenResponse CreateResponse(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var now = DateTimeOffset.UtcNow;
            return new AuthenticationTokenResponse(Guid.NewGuid(), "person@example.test", "Test", "User",
                Array.Empty<string>(), "access-token", "refresh-token", "Bearer", now.AddMinutes(15),
                now.AddDays(30), Guid.NewGuid(), Guid.NewGuid());
        }
    }
}
