using Foodiya.Domain.Configuration;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Foodiya.API.Extensions;

internal static class AuthenticationServiceCollectionExtensions
{
    public static IServiceCollection AddIdentityAuthentication(this IServiceCollection services,
        IConfiguration configuration, IHostEnvironment environment)
    {
        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(options => Uri.TryCreate(options.Authority, UriKind.Absolute, out var authority)
                && (authority.Scheme == Uri.UriSchemeHttps
                    || environment.IsDevelopment() && authority.Scheme == Uri.UriSchemeHttp)
                && Uri.TryCreate(options.Issuer, UriKind.Absolute, out var issuer)
                && (issuer.Scheme == Uri.UriSchemeHttps
                    || environment.IsDevelopment() && issuer.Scheme == Uri.UriSchemeHttp)
                && !string.IsNullOrWhiteSpace(options.Audience),
                "Authentication:Jwt requires an HTTPS Authority, Issuer and Audience. HTTP is allowed only in Development.")
            .ValidateOnStart();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                var settings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new();
                options.Authority = settings.Authority;
                options.RequireHttpsMetadata = !environment.IsDevelopment();
                options.MapInboundClaims = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = settings.Issuer,
                    ValidateAudience = true,
                    ValidAudience = settings.Audience,
                    ValidateIssuerSigningKey = true,
                    ValidAlgorithms = [SecurityAlgorithms.RsaSha256],
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                    NameClaimType = "name",
                    RoleClaimType = "role"
                };
            });
        services.AddAuthorization();
        return services;
    }
}
