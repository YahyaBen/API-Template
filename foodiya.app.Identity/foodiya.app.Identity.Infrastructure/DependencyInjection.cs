using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using foodiya.app.Identity.Application.Interfaces;
using foodiya.app.Identity.Infrastructure.Accounts;
using foodiya.app.Identity.Infrastructure.Devices;
using foodiya.app.Identity.Infrastructure.Authentication;
using foodiya.app.Identity.Infrastructure.Authorization;
using foodiya.app.Identity.Infrastructure.Identity;
using foodiya.app.Identity.Infrastructure.Options;
using foodiya.app.Identity.Infrastructure.Persistence;
using foodiya.app.Identity.Infrastructure.Registration;
using foodiya.app.Identity.Infrastructure.Operations;
using Microsoft.IdentityModel.Tokens;
using foodiya.app.Messaging;
using foodiya.app.Identity.Domain.Constants;

namespace foodiya.app.Identity.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddIdentityInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        IHostEnvironment environment)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(environment);

        var connectionString = configuration.GetConnectionString("IdentityDatabase")
            ?? throw new InvalidOperationException(
                "Connection string 'IdentityDatabase' is required.");

        services.AddDbContext<IdentityDataContext>(options =>
            options.UseSqlServer(connectionString, sqlOptions =>
                sqlOptions.MigrationsAssembly(typeof(IdentityDataContext).Assembly.FullName)));

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireDigit = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredUniqueChars = 1;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddRoles<IdentityRole<Guid>>()
            .AddEntityFrameworkStores<IdentityDataContext>();

        services.AddOptions<RegistrationOptions>()
            .Bind(configuration.GetSection(RegistrationOptions.SectionName))
            .Validate(
                options => options.OtpPepper.Length >= 32,
                "Registration:OtpPepper must contain at least 32 characters.")
            .Validate(
                options => options.OtpLifetimeMinutes is >= 1 and <= 30,
                "Registration:OtpLifetimeMinutes must be between 1 and 30.")
            .Validate(
                options => options.OtpMaxAttempts is >= 1 and <= 10,
                "Registration:OtpMaxAttempts must be between 1 and 10.")
            .Validate(
                options => options.OtpResendCooldownSeconds is >= 10 and <= 600,
                "Registration:OtpResendCooldownSeconds must be between 10 and 600.")
            .ValidateOnStart();

        services.AddOptions<AuthenticationTokenOptions>()
            .Bind(configuration.GetSection(AuthenticationTokenOptions.SectionName))
            .Validate(
                options => Uri.TryCreate(options.Issuer, UriKind.Absolute, out var issuer)
                    && (environment.IsDevelopment() || issuer.Scheme == Uri.UriSchemeHttps),
                "Tokens:Issuer must be an absolute HTTPS URI outside Development.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.Audience),
                "Tokens:Audience is required.")
            .Validate(
                options => !string.IsNullOrWhiteSpace(options.KeyId),
                "Tokens:KeyId is required.")
            .Validate(
                options => options.RefreshTokenPepper.Length >= 32,
                "Tokens:RefreshTokenPepper must contain at least 32 characters.")
            .Validate(
                options => options.AccessTokenLifetimeMinutes is >= 1 and <= 60,
                "Tokens:AccessTokenLifetimeMinutes must be between 1 and 60.")
            .Validate(
                options => options.RefreshTokenLifetimeDays is >= 1 and <= 90,
                "Tokens:RefreshTokenLifetimeDays must be between 1 and 90.")
            .ValidateOnStart();

        services.AddOptions<GoogleAuthenticationOptions>()
            .Bind(configuration.GetSection(GoogleAuthenticationOptions.SectionName))
            .Validate(
                options => options.ClientIds.All(clientId =>
                    !string.IsNullOrWhiteSpace(clientId)
                    && clientId.EndsWith(".apps.googleusercontent.com", StringComparison.Ordinal)),
                "Authentication:Google:ClientIds must contain valid Google OAuth client IDs.")
            .ValidateOnStart();

        services.AddOptions<AccountLifecycleOptions>()
            .Bind(configuration.GetSection(AccountLifecycleOptions.SectionName))
            .Validate(options => options.PasswordHistoryCount is >= 1 and <= 24,
                "AccountLifecycle:PasswordHistoryCount must be between 1 and 24.")
            .ValidateOnStart();

        services.AddOptions<BootstrapAdminOptions>()
            .Bind(configuration.GetSection(BootstrapAdminOptions.SectionName))
            .Validate(
                options => !options.Enabled || !string.IsNullOrWhiteSpace(options.Email),
                "BootstrapAdmin:Email is required when bootstrap is enabled.")
            .Validate(
                options => !options.Enabled || !string.IsNullOrWhiteSpace(options.UserName),
                "BootstrapAdmin:UserName is required when bootstrap is enabled.")
            .Validate(
                options => !options.Enabled || options.Password.Length >= 8,
                "BootstrapAdmin:Password must contain at least 8 characters when bootstrap is enabled.")
            .Validate(
                options => !options.Enabled
                    || (!string.IsNullOrWhiteSpace(options.FirstName)
                        && !string.IsNullOrWhiteSpace(options.LastName)),
                "BootstrapAdmin first and last names are required when bootstrap is enabled.")
            .ValidateOnStart();

        services.AddOptions<CleanupOptions>()
            .Bind(configuration.GetSection(CleanupOptions.SectionName))
            .Validate(options => options.IntervalMinutes is >= 1 and <= 1440,
                "Cleanup:IntervalMinutes must be between 1 and 1440.")
            .Validate(options => options.BatchSize is >= 1 and <= 5000,
                "Cleanup:BatchSize must be between 1 and 5000.")
            .Validate(options => options.SessionRetentionDays is >= 1 and <= 3650,
                "Cleanup:SessionRetentionDays must be between 1 and 3650.")
            .Validate(options => options.OtpRetentionDays is >= 1 and <= 365,
                "Cleanup:OtpRetentionDays must be between 1 and 365.")
            .Validate(options => options.ProcessedEventRetentionDays is >= 1 and <= 3650,
                "Cleanup:ProcessedEventRetentionDays must be between 1 and 3650.")
            .Validate(options => options.RegistrationAttemptRetentionDays is >= 1 and <= 365,
                "Cleanup:RegistrationAttemptRetentionDays must be between 1 and 365.")
            .Validate(options => options.DeletedAccountRetentionDays is >= 1 and <= 3650,
                "Cleanup:DeletedAccountRetentionDays must be between 1 and 3650.")
            .ValidateOnStart();

        services.AddOptions<OutboxOptions>()
            .Bind(configuration.GetSection(OutboxOptions.SectionName))
            .Validate(options => options.Transport is "Webhook" or "RabbitMq",
                "Outbox:Transport must be Webhook or RabbitMq.")
            .Validate(options => !options.Enabled || options.Transport != "Webhook" || Uri.TryCreate(options.Endpoint, UriKind.Absolute, out _),
                "Outbox:Endpoint must be an absolute URI when delivery is enabled.")
            .Validate(options => !options.Enabled || options.Transport != "Webhook" || options.SigningSecret.Length >= 32,
                "Outbox:SigningSecret must contain at least 32 characters when delivery is enabled.")
            .Validate(options => options.IntervalSeconds is >= 1 and <= 3600,
                "Outbox:IntervalSeconds must be between 1 and 3600.")
            .Validate(options => options.BatchSize is >= 1 and <= 500,
                "Outbox:BatchSize must be between 1 and 500.")
            .Validate(options => options.MaxAttempts is >= 1 and <= 100,
                "Outbox:MaxAttempts must be between 1 and 100.")
            .Validate(options => options.LeaseSeconds is >= 30 and <= 3600,
                "Outbox:LeaseSeconds must be between 30 and 3600.")
            .Validate(options => options.TimeoutSeconds is >= 1 and <= 300,
                "Outbox:TimeoutSeconds must be between 1 and 300.")
            .Validate(options => options.TimeoutSeconds < options.LeaseSeconds,
                "Outbox:TimeoutSeconds must be shorter than Outbox:LeaseSeconds.")
            .ValidateOnStart();

        services.AddOptions<EmailDeliveryOptions>()
            .Bind(configuration.GetSection(EmailDeliveryOptions.SectionName))
            .Validate(options => options.Provider is "Disabled" or "Development" or "Smtp",
                "EmailDelivery:Provider must be Disabled, Development, or Smtp.")
            .Validate(options => options.Provider != "Smtp" || !string.IsNullOrWhiteSpace(options.Host),
                "EmailDelivery:Host is required for SMTP.")
            .Validate(options => options.Provider != "Smtp" || MimeKit.MailboxAddress.TryParse(options.FromAddress, out _),
                "EmailDelivery:FromAddress must be valid for SMTP.")
            .Validate(options => options.Port is >= 1 and <= 65535,
                "EmailDelivery:Port must be between 1 and 65535.")
            .Validate(options => options.TimeoutSeconds is >= 1 and <= 300,
                "EmailDelivery:TimeoutSeconds must be between 1 and 300.")
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var tokenOptions = configuration
                    .GetSection(AuthenticationTokenOptions.SectionName)
                    .Get<AuthenticationTokenOptions>() ?? new AuthenticationTokenOptions();
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = tokenOptions.Issuer,
                    ValidateAudience = true,
                    ValidAudience = tokenOptions.Audience,
                    ValidateIssuerSigningKey = true,
                    ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                    ValidateLifetime = true,
                    RequireExpirationTime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
                options.Events = new JwtBearerEvents
                {
                    OnTokenValidated = async context =>
                    {
                        if (context.Principal is null)
                        {
                            context.Fail("The access token has no authenticated principal.");
                            return;
                        }

                        var validator = context.HttpContext.RequestServices
                            .GetRequiredService<IAccessSessionValidator>();
                        var access = await validator.ValidateAsync(
                            context.Principal,
                            context.HttpContext.RequestAborted);
                        if (access is null)
                        {
                            context.Fail("The access session is no longer valid.");
                        }
                        else
                        {
                            context.HttpContext.Features.Set(
                                new AuthenticatedAccessSession(context.Principal, access));
                        }
                    },
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                        context.Response.ContentType = "application/problem+json";
                        context.Response.Headers.WWWAuthenticate = "Bearer";
                        var problems = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
                        await problems.TryWriteAsync(new ProblemDetailsContext
                        {
                            HttpContext = context.HttpContext,
                            ProblemDetails = CreateAuthenticationProblem(
                                context.HttpContext,
                                StatusCodes.Status401Unauthorized,
                                "Authentication required",
                                "A valid active-session bearer token is required.",
                                "AUTHENTICATION_REQUIRED")
                        });
                    },
                    OnForbidden = async context =>
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/problem+json";
                        var problems = context.HttpContext.RequestServices.GetRequiredService<IProblemDetailsService>();
                        await problems.TryWriteAsync(new ProblemDetailsContext
                        {
                            HttpContext = context.HttpContext,
                            ProblemDetails = CreateAuthenticationProblem(
                                context.HttpContext,
                                StatusCodes.Status403Forbidden,
                                "Insufficient permissions",
                                "The authenticated account does not satisfy the required role policy.",
                                "AUTHORIZATION_FORBIDDEN")
                        });
                    }
                };
            });

        services.AddOptions<JwtBearerOptions>(JwtBearerDefaults.AuthenticationScheme)
            .Configure<IRsaSigningKeyProvider>((options, keyProvider) =>
            {
                options.TokenValidationParameters.IssuerSigningKey = keyProvider.GetValidationKey();
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(
                AuthorizationPolicyNames.Chef,
                policy => policy.RequireRole(
                    RoleNames.Chef,
                    RoleNames.Admin,
                    RoleNames.SuperAdmin));
            options.AddPolicy(
                AuthorizationPolicyNames.Admin,
                policy => policy.RequireRole(RoleNames.Admin, RoleNames.SuperAdmin));
            options.AddPolicy(
                AuthorizationPolicyNames.SuperAdmin,
                policy => policy.RequireRole(RoleNames.SuperAdmin));
        });

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IRsaSigningKeyProvider, RsaSigningKeyProvider>();
        services.AddSingleton<IOtpCodeService, HmacOtpCodeService>();
        services.AddSingleton<IAccessTokenService, JwtAccessTokenService>();
        services.AddSingleton<ITokenCryptographyService, TokenCryptographyService>();
        services.AddSingleton<ILoginTimingProtection, LoginTimingProtection>();
        services.AddSingleton<IGoogleIdTokenValidator, GoogleIdTokenValidator>();
        services.AddSingleton<IdentityTelemetry>();
        services.AddHttpContextAccessor();
        services.AddScoped<IAccessSessionValidator, AccessSessionValidator>();
        var emailProvider = configuration[$"{EmailDeliveryOptions.SectionName}:Provider"] ?? "Disabled";
        services.AddScoped<IIdentityEmailTransport, MailKitIdentityEmailTransport>();
        services.AddScoped<IIdentityEmailSender>(emailProvider.Equals("Smtp", StringComparison.OrdinalIgnoreCase)
            ? static provider => ActivatorUtilities.CreateInstance<SmtpIdentityEmailSender>(provider)
            : static provider => ActivatorUtilities.CreateInstance<DevelopmentIdentityEmailSender>(provider));
        services.AddScoped<IRegistrationService, RegistrationService>();
        services.AddScoped<IIdentityAuthenticationService, IdentityAuthenticationService>();
        services.AddScoped<ICurrentIdentityService, CurrentIdentityService>();
        services.AddScoped<IRoleManagementService, RoleManagementService>();
        services.AddScoped<IAccountLifecycleService, AccountLifecycleService>();
        services.AddScoped<IDeviceSessionService, DeviceSessionService>();
        services.AddScoped<IdentityCleanupService>();
        services.AddScoped<OutboxDispatcher>();
        services.AddRabbitMqMessaging(configuration, configuration.GetValue<bool>("Outbox:Enabled")
            && configuration["Outbox:Transport"] == "RabbitMq");
        if (configuration["Outbox:Transport"] == "RabbitMq")
            services.AddScoped<IOutboxTransport, RabbitMqOutboxTransport>();
        else
            services.AddScoped<IOutboxTransport, WebhookOutboxTransport>();
        services.AddHttpClient("IdentityOutbox", (provider, client) =>
        {
            var settings = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<OutboxOptions>>().Value;
            client.Timeout = TimeSpan.FromSeconds(settings.TimeoutSeconds);
        });
        services.AddHostedService<SuperAdminBootstrapHostedService>();
        services.AddHostedService<CleanupBackgroundService>();
        services.AddHostedService<OutboxBackgroundService>();

        return services;
    }

    private static ProblemDetails CreateAuthenticationProblem(
        HttpContext httpContext,
        int status,
        string title,
        string detail,
        string code) => new()
        {
            Status = status,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path,
            Extensions =
            {
                ["code"] = code,
                ["traceId"] = httpContext.TraceIdentifier
            }
        };
}
