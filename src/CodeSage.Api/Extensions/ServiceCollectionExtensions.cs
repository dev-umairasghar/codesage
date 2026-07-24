using System.Reflection;
using CodeSage.Api.Swagger;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OpenApi;

namespace CodeSage.Api.Extensions;

/// <summary>
/// API-host service registration kept out of Program.cs for readability.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers API concerns: OpenAPI/Swagger, compression, and problem details.
    /// </summary>
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddCors(options =>
        {
            options.AddPolicy(
                "LocalWeb",
                policy =>
                    policy
                        .WithOrigins(
                            "http://localhost:5173",
                            "http://127.0.0.1:5173")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
        });
        services.AddResponseCompression(options =>
        {
            options.EnableForHttps = true;
            options.Providers.Add<BrotliCompressionProvider>();
            options.Providers.Add<GzipCompressionProvider>();
        });

        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "CodeSage API",
                Version = "v1",
                Description =
                    """
                    Local-first, stateless AI pull request review assistant.

                    ## Authentication
                    No user authentication. Configure `GitHub:PersonalAccessToken` and `OpenAI:ApiKey`
                    via user-secrets or environment variables.

                    ## Errors
                    All failures return RFC 7807 `application/problem+json` with an `errorCode` extension.

                    ## Versioning
                    Current surface is `/api/v1`. Breaking changes will ship as `/api/v2`.
                    """,
                Contact = new OpenApiContact
                {
                    Name = "CodeSage Contributors",
                    Url = new Uri("https://github.com/codesage")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT"
                }
            });

            options.SupportNonNullableReferenceTypes();
            options.DescribeAllParametersInCamelCase();
            options.OperationFilter<ProblemDetailsOperationFilter>();
            options.SchemaFilter<ExampleSchemaFilter>();

            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            if (File.Exists(xmlPath))
            {
                options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
            }

            var contractsXml = Path.Combine(AppContext.BaseDirectory, "CodeSage.Contracts.xml");
            if (File.Exists(contractsXml))
            {
                options.IncludeXmlComments(contractsXml);
            }

            var applicationXml = Path.Combine(AppContext.BaseDirectory, "CodeSage.Application.xml");
            if (File.Exists(applicationXml))
            {
                options.IncludeXmlComments(applicationXml);
            }
        });

        return services;
    }
}
