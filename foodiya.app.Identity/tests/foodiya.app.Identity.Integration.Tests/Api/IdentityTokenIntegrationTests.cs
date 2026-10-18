extern alias FoodiyaApi;

using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using Foodiya.Infrastructure.Data;
using foodiya.app.Identity.Contracts.Authentication;
using foodiya.app.Identity.Domain.Enums;
using foodiya.app.Identity.Infrastructure.Identity;
using foodiya.app.Identity.Infrastructure.Persistence;
using foodiya.app.Identity.Integration.Tests.Messaging;
using foodiya.app.Identity.Integration.Tests.TestSupport;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace foodiya.app.Identity.Integration.Tests.Api;

[TestClass]
public sealed class IdentityTokenIntegrationTests(TestContext testContext)
{
    private const string Issuer = "https://identity.integration.test";
    private const string Audience = "foodiya.app.Identity.Clients";

    [TestMethod]
    public async Task IdentityLoginToken_IsAcceptedByProtectedFoodiyaEndpointThroughDiscovery()
    {
        await using var harness = await CreateAsync(testContext.CancellationToken);
        using var response = await harness.Api.GetAsync("/api/Unit", testContext.CancellationToken);
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode,
            await response.Content.ReadAsStringAsync(testContext.CancellationToken));
        await using var db = harness.Profiles.CreateContext();
        var profile = await db.AppUsers.SingleAsync(testContext.CancellationToken);
        Assert.AreEqual(harness.Subject, profile.Uuid);
        Assert.AreEqual("token@example.test", profile.Email);
        Assert.AreEqual("Juan Carlos", profile.FirstName);
        Assert.AreEqual("العلمي", profile.LastName);
    }

    [TestMethod]
    public async Task UserToken_CannotAccessAdministratorEndpoint()
    {
        await using var harness = await CreateAsync(testContext.CancellationToken);
        using var response = await harness.Api.PostAsJsonAsync("/api/Unit", new { }, testContext.CancellationToken);
        Assert.AreEqual(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [TestMethod]
    [DataRow("issuer")]
    [DataRow("audience")]
    [DataRow("key")]
    [DataRow("algorithm")]
    [DataRow("expired")]
    public async Task UntrustedToken_IsRejectedWithoutProvisioningProfile(string invalidPart)
    {
        await using var harness = await CreateAsync(testContext.CancellationToken);
        using var rsa = RSA.Create(2048);
        var key = invalidPart == "algorithm"
            ? (SecurityKey)new SymmetricSecurityKey(RandomNumberGenerator.GetBytes(32))
            : new RsaSecurityKey(rsa) { KeyId = "untrusted-key" };
        var descriptor = new SecurityTokenDescriptor
        {
            Issuer = invalidPart == "issuer" ? "https://wrong.example.test" : Issuer,
            Audience = invalidPart == "audience" ? "wrong-clients" : Audience,
            NotBefore = DateTime.UtcNow.AddHours(-1), IssuedAt = DateTime.UtcNow.AddHours(-1),
            Expires = DateTime.UtcNow.AddMinutes(invalidPart == "expired" ? -5 : 5),
            Subject = new ClaimsIdentity([new Claim("sub", Guid.NewGuid().ToString()), new Claim("email", "wrong@example.test")]),
            SigningCredentials = new SigningCredentials(key,
                invalidPart == "algorithm" ? SecurityAlgorithms.HmacSha256 : SecurityAlgorithms.RsaSha256)
        };
        // Wrong issuer/audience cases retain the real Identity signature, so
        // their rejection cannot be explained by an unrelated signing key.
        if (invalidPart is "issuer" or "audience" or "expired")
            descriptor.SigningCredentials = new SigningCredentials(harness.IdentityFactory.Services
                .GetRequiredService<foodiya.app.Identity.Infrastructure.Authentication.IRsaSigningKeyProvider>().GetSigningKey(), SecurityAlgorithms.RsaSha256);
        harness.Api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer",
            new JwtSecurityTokenHandler().CreateEncodedJwt(descriptor));
        using var response = await harness.Api.GetAsync("/api/Unit", testContext.CancellationToken);
        Assert.AreEqual(HttpStatusCode.Unauthorized, response.StatusCode);
        await using var db = harness.Profiles.CreateContext();
        Assert.AreEqual(0, await db.AppUsers.CountAsync(testContext.CancellationToken));

    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ExistingProfile_RequestDoesNotRelinkOrReactivate(bool differentSubject)
    {
        await using var harness = await CreateAsync(testContext.CancellationToken);
        await using var db = harness.Profiles.CreateContext();
        var originalSubject = differentSubject ? Guid.NewGuid() : harness.Subject;
        var profile = await new Foodiya.Infrastructure.Identity.IdentityProfileProvisioner(db, TimeProvider.System)
            .EnsureAsync(new foodiya.app.Messaging.UserRegisteredEventPayload(originalSubject, "token@example.test"), null, testContext.CancellationToken);
        profile.IsActive = false;
        profile.FirstName = "Edited";
        await db.SaveChangesAsync(testContext.CancellationToken);
        using var response = await harness.Api.GetAsync("/api/Unit", testContext.CancellationToken);
        Assert.AreEqual(differentSubject ? HttpStatusCode.Conflict : HttpStatusCode.Forbidden, response.StatusCode);
        db.ChangeTracker.Clear();
        var unchanged = await db.AppUsers.SingleAsync(testContext.CancellationToken);
        Assert.AreEqual(originalSubject, unchanged.Uuid);
        Assert.IsFalse(unchanged.IsActive);
        Assert.AreEqual("Edited", unchanged.FirstName);
    }

    private static async Task<Harness> CreateAsync(CancellationToken cancellationToken)
    {
        var anchor = new SqliteConnection($"Data Source=token-identity-{Guid.NewGuid():N};Mode=Memory;Cache=Shared;Pooling=false");
        await anchor.OpenAsync(cancellationToken);
        var identityFactory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Tokens:Issuer"] = Issuer, ["Tokens:Audience"] = Audience,
                ["Tokens:RsaPrivateKeyPem"] = "", ["Tokens:RsaPrivateKeyPath"] = "",
                ["EmailDelivery:Provider"] = "Disabled", ["BootstrapAdmin:Enabled"] = "false",
                ["Cleanup:Enabled"] = "false", ["Outbox:Enabled"] = "false"
            }));
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IdentityDataContext>();
                services.RemoveAll<DbContextOptions<IdentityDataContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<IdentityDataContext>>();
                services.AddDbContext<IdentityDataContext>(options => options.UseSqlite(anchor.ConnectionString)
                    .ReplaceService<IModelCustomizer, SqliteTestModelCustomizer>());
            });
        });
        var identity = identityFactory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri(Issuer) });
        var subject = Guid.NewGuid();
        await using (var scope = identityFactory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<IdentityDataContext>();
            await db.Database.EnsureCreatedAsync(cancellationToken);
            var users = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = new ApplicationUser
            {
                Id = subject, Email = "token@example.test", UserName = "token.user", FirstName = "Juan Carlos", LastName = "العلمي",
                EmailConfirmed = true, Status = AccountStatus.Active, CreatedAtUtc = DateTimeOffset.UtcNow, LockoutEnabled = true
            };
            Assert.IsTrue((await users.CreateAsync(user, "Valid!Password123")).Succeeded);
            Assert.IsTrue((await users.AddToRoleAsync(user, "USER")).Succeeded);
        }
        using var login = await identity.PostAsJsonAsync("/api/auth/login", new LoginRequest
        {
            Login = "token.user", Password = "Valid!Password123", DeviceInstallationId = "token-test-device"
        }, cancellationToken);
        Assert.AreEqual(HttpStatusCode.OK, login.StatusCode, await login.Content.ReadAsStringAsync(cancellationToken));
        var tokens = await login.Content.ReadFromJsonAsync<AuthenticationTokenResponse>(cancellationToken);
        Assert.IsNotNull(tokens);

        var profiles = new ProfileTestDatabase();
        await profiles.InitializeAsync(cancellationToken);
        var apiFactory = new WebApplicationFactory<FoodiyaApi::Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureLogging(logging => logging.ClearProviders());
            builder.ConfigureAppConfiguration((_, config) => config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Authentication:Jwt:Authority"] = Issuer, ["Authentication:Jwt:Issuer"] = Issuer,
                ["Authentication:Jwt:Audience"] = Audience, ["IdentityEvents:Enabled"] = "false",
                ["ApplicationInsights:ConnectionString"] = ""
            }));
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<AutoGeneratedDbContext>();
                services.RemoveAll<DbContextOptions<AutoGeneratedDbContext>>();
                services.RemoveAll<IDbContextOptionsConfiguration<AutoGeneratedDbContext>>();
                services.AddDbContext<AutoGeneratedDbContext>(options => options.UseSqlite(profiles.Connection.ConnectionString)
                    .ReplaceService<IModelCustomizer, ProfileSqliteModelCustomizer>());
                // Keep the real bearer handler and discovery/JWKS parsing. Only
                // route its network requests to the in-process Identity server.
                services.Configure<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme,
                    options => options.Backchannel = identityFactory.CreateClient(new WebApplicationFactoryClientOptions { BaseAddress = new Uri(Issuer) }));
            });
        });
        var api = apiFactory.CreateClient();
        api.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", tokens.AccessToken);
        return new Harness(anchor, identityFactory, apiFactory, identity, api, profiles, subject);
    }

    private sealed record Harness(SqliteConnection Anchor, WebApplicationFactory<Program> IdentityFactory,
        WebApplicationFactory<FoodiyaApi::Program> ApiFactory, HttpClient Identity, HttpClient Api,
        ProfileTestDatabase Profiles, Guid Subject) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync()
        {
            Api.Dispose(); Identity.Dispose();
            await ApiFactory.DisposeAsync(); await IdentityFactory.DisposeAsync();
            await Profiles.DisposeAsync(); await Anchor.DisposeAsync();
        }
    }
}
