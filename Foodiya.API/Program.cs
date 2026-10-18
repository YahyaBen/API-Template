using Foodiya.API.Middleware;
using Foodiya.API.Extensions;
using Foodiya.Infrastructure.Extensions;
using Foodiya.Application.Extensions;
using Microsoft.OpenApi;
using Scalar.AspNetCore;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        builder.Configuration.AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true);
        builder.Configuration.AddEnvironmentVariables();
        builder.Services.AddIdentityAuthentication(builder.Configuration, builder.Environment);

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Foodiya API",
                Version = "v1",
                Description = "API for managing recipes, ingredients, and reference data.",
                Contact = new OpenApiContact
        {
            Name = "API Yahya BENABDALLAH Support",
            Email = "support@example.com",
            Url = new Uri("https://example.com/support")
        },
        License = new OpenApiLicense
        {
            Name = "Yahya License B Leflous",
            Url = new Uri("https://opensource.org/licenses/MIT")
        }
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Enter a valid JWT bearer token."
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecuritySchemeReference("Bearer", document, null),
                    []
                }
            });

            // Include XML comments from the API project
            var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
                options.IncludeXmlComments(xmlPath);
        });
        if (!string.IsNullOrWhiteSpace(builder.Configuration["ApplicationInsights:ConnectionString"]))
            builder.Services.AddApplicationInsightsTelemetry(builder.Configuration);
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
        builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.IsDevelopment());
        builder.Services.AddServices();
        builder.Services.AddCors();

        var app = builder.Build();

        app.UseExceptionHandler();

        var corsOrigins = app.Configuration.GetSection("CorsOrigins").Get<string[]>() ?? [];
        app.UseCors(policy => policy.AllowAnyHeader().AllowAnyMethod().WithOrigins(corsOrigins));

        app.UseSwagger();
        app.UseSwaggerUI();

        if (app.Environment.IsDevelopment())
        {
            app.MapScalarApiReference(options => options
                .WithTitle("Foodiya API Reference")
                .WithTheme(ScalarTheme.BluePlanet)
                .WithOpenApiRoutePattern("/swagger/{documentName}/swagger.json")
                .AddPreferredSecuritySchemes(["Bearer"]));
        }

        app.UseAuthentication();
        app.UseMiddleware<IdentityProfileProvisioningMiddleware>();
        app.UseAuthorization();

        app.MapControllers();

        app.MapGet("/health", () => Results.Ok("Healthy"));
        app.MapHealthChecks("/health/ready");

        app.Run();
    }

}
