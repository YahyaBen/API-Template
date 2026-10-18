using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;
using System.Text.Json.Nodes;

namespace foodiya.app.Identity.Api.OpenApi;

internal sealed class IdentityOpenApiOperationTransformer : IOpenApiOperationTransformer
{
    public Task TransformAsync(
        OpenApiOperation operation,
        OpenApiOperationTransformerContext context,
        CancellationToken cancellationToken)
    {
        operation.Parameters ??= [];
        if (!operation.Parameters.Any(parameter =>
                parameter.In == ParameterLocation.Header
                && string.Equals(parameter.Name, "Accept-Language", StringComparison.OrdinalIgnoreCase)))
        {
            operation.Parameters.Add(new OpenApiParameter
            {
                Name = "Accept-Language",
                In = ParameterLocation.Header,
                Required = false,
                Description = "Response language. Supported values: en, fr, ar.",
                Schema = new OpenApiSchema
                {
                    Type = JsonSchemaType.String,
                    Default = JsonValue.Create("en"),
                    Enum = [JsonValue.Create("en"), JsonValue.Create("fr"), JsonValue.Create("ar")]
                }
            });
        }

        var endpointMetadata = context.Description.ActionDescriptor.EndpointMetadata;
        var requiresAuthorization = endpointMetadata.OfType<IAuthorizeData>().Any()
            && !endpointMetadata.OfType<IAllowAnonymous>().Any();

        if (requiresAuthorization)
        {
            operation.Security ??= [];
            operation.Security.Add(new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference(JwtBearerDefaults.AuthenticationScheme, context.Document)] = []
            });
        }

        return Task.CompletedTask;
    }
}
