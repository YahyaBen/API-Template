using System.Text.Json.Serialization;
using System.Globalization;
using Microsoft.AspNetCore.Localization;
using Scalar.AspNetCore;
using foodiya.app.Identity.Api;
using foodiya.app.Identity.Api.Endpoints;
using foodiya.app.Identity.Api.Middleware;
using foodiya.app.Identity.Api.OpenApi;
using foodiya.app.Identity.Contracts;
using foodiya.app.Identity.Infrastructure;
using foodiya.app.Identity.Infrastructure.Authentication;
using ApiTemplate.Hosting;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddApiHosting(builder.Configuration, builder.Environment);
builder.Services.AddHealthChecks().AddCheck<SqlReadinessCheck>("database", tags: ["ready"], timeout: TimeSpan.FromSeconds(5));

builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = IdentityProblemDetailsDefaults.Customize);
builder.Services.AddExceptionHandler<IdentityApiExceptionHandler>();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<BearerSecuritySchemeTransformer>();
    options.AddOperationTransformer<IdentityOpenApiOperationTransformer>();
});
builder.Services.AddValidation();
builder.Services.ConfigureHttpJsonOptions(options =>
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));
builder.Services.AddIdentityInfrastructure(builder.Configuration, builder.Environment);
builder.Services.AddIdentityObservability(builder.Configuration);
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    var cultures = new[] { new CultureInfo("en"), new CultureInfo("fr"), new CultureInfo("ar") };
    options.DefaultRequestCulture = new RequestCulture("en");
    options.SupportedCultures = cultures;
    options.SupportedUICultures = cultures;
});
builder.Services.AddIdentityRateLimiting(builder.Configuration);

var app = builder.Build();
_ = app.Services.GetRequiredService<IRsaSigningKeyProvider>();
// Middlewares
app.UseForwardedHeaders();
app.UseRequestLocalization();
app.UseMiddleware<IdentityAuditMiddleware>();
app.UseExceptionHandler();
app.UseCors();
app.UseStatusCodePages();
app.UseMiddleware<IngressRateLimitMiddleware>();
app.UseAuthentication();
app.UseRateLimiter();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options.WithTitle("Foodiya App Identity API Reference")
            .WithTheme(ScalarTheme.BluePlanet)
            .AddPreferredSecuritySchemes(["Bearer"]);
        // .WithTheme(ScalarTheme.Laserwave);
        // .WithTheme(ScalarTheme.Solarized);
        // .WithTheme(ScalarTheme.DeepSpace);
        // .WithTheme(ScalarTheme.Moon);
        // .WithTheme(ScalarTheme.Kepler);
    });
}

app.MapGet("/health", () => TypedResults.Ok(new
{
    Status = "Healthy",
    CheckedAt = DateTimeOffset.UtcNow
}));
app.MapHealthChecks("/health/ready");

app.MapGet("/foodiya-app-identity", () =>
    TypedResults.Ok(new IdentityServiceMetadata(
        Service: "foodiya.app.Identity",
        Version: "0.0.1",
        Status: "Operational")));

app.MapRegistrationEndpoints();
app.MapAuthenticationEndpoints();
app.MapOidcDiscoveryEndpoints();
app.MapUserEndpoints();
app.MapRoleEndpoints();
app.MapAccountEndpoints();
app.MapDeviceEndpoints();

app.Run();

public partial class Program;
