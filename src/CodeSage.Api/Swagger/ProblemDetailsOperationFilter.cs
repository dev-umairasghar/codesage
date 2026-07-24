using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CodeSage.Api.Swagger;

/// <summary>
/// Ensures ProblemDetails responses are documented for common error statuses.
/// </summary>
public sealed class ProblemDetailsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(operation);
        ArgumentNullException.ThrowIfNull(context);

        operation.Responses ??= new OpenApiResponses();

        EnsureResponse(
            operation,
            "400",
            "Validation failed — RFC 7807 ProblemDetails with an errors object.");
        EnsureResponse(
            operation,
            "500",
            "Unexpected server error — RFC 7807 ProblemDetails.");
    }

    private static void EnsureResponse(OpenApiOperation operation, string statusCode, string description)
    {
        if (operation.Responses!.ContainsKey(statusCode))
        {
            return;
        }

        operation.Responses[statusCode] = new OpenApiResponse
        {
            Description = description,
            Content = new Dictionary<string, OpenApiMediaType>
            {
                ["application/problem+json"] = new OpenApiMediaType
                {
                    Schema = new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
                        Properties = new Dictionary<string, IOpenApiSchema>
                        {
                            ["type"] = new OpenApiSchema { Type = JsonSchemaType.String },
                            ["title"] = new OpenApiSchema { Type = JsonSchemaType.String },
                            ["status"] = new OpenApiSchema { Type = JsonSchemaType.Integer },
                            ["detail"] = new OpenApiSchema { Type = JsonSchemaType.String },
                            ["instance"] = new OpenApiSchema { Type = JsonSchemaType.String },
                            ["traceId"] = new OpenApiSchema { Type = JsonSchemaType.String },
                            ["errorCode"] = new OpenApiSchema { Type = JsonSchemaType.String }
                        }
                    }
                }
            }
        };
    }
}
