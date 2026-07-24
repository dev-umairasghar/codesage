using CodeSage.Application.Features.AI.Models;
using CodeSage.Application.Features.Analysis.Models;
using CodeSage.Contracts.Configuration;
using CodeSage.Contracts.Health;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CodeSage.Api.Swagger;

/// <summary>
/// Adds illustrative OpenAPI schema examples for primary response contracts.
/// </summary>
public sealed class ExampleSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        ArgumentNullException.ThrowIfNull(schema);
        ArgumentNullException.ThrowIfNull(context);

        if (schema is not OpenApiSchema concrete)
        {
            return;
        }

        if (context.Type == typeof(HealthResponse))
        {
            concrete.Example = """
                {"status":"Healthy","application":"CodeSage","version":"0.1.0"}
                """;
        }
        else if (context.Type == typeof(ConfigurationSummaryResponse))
        {
            concrete.Example = """
                {"application":"CodeSage","version":"0.1.0","environment":"Development","gitHubApiBaseUrl":"https://api.github.com/","gitHubTokenConfigured":true,"aiProvider":"OpenAI","aiModel":"gpt-4o-mini","openAiBaseUrl":"https://api.openai.com/v1/","openAiApiKeyConfigured":true,"probeExternalConnectivity":true,"requireSecretsAtStartup":true}
                """;
        }
        else if (context.Type == typeof(SystemStatusResponse))
        {
            concrete.Example = """
                {"application":"CodeSage","version":"0.1.0","environment":"Development","aiProvider":"OpenAI","aiModel":"gpt-4o-mini","gitHubTokenConfigured":true,"openAiApiKeyConfigured":true,"gitHubConnectivity":{"status":"Skipped","message":"External probes disabled"},"openAiConnectivity":{"status":"Skipped","message":"External probes disabled"},"diagnostics":["Configuration looks ready for local reviews."]}
                """;
        }
        else if (context.Type == typeof(ReviewReport))
        {
            concrete.Example = """
                {"summary":"Solid incremental change with one auth edge case.","overallRisk":"Medium","positiveFindings":["Clear PR description"],"issues":[],"recommendations":["Add a regression test for the null token path"],"missingTests":["Auth failure path"],"securityConcerns":[],"performanceConcerns":[],"maintainability":[],"architectureConcerns":[],"model":"gpt-4o-mini","promptTokens":1200,"completionTokens":400,"totalTokens":1600,"duration":"00:00:02.1500000"}
                """;
        }
        else if (context.Type == typeof(ReviewContext))
        {
            concrete.Example = """
                {"repository":{"name":"Healthcare.API","fullName":"acme/Healthcare.API","defaultBranch":"main"},"pullRequest":{"number":42,"title":"Harden auth","description":"...","state":"open","isDraft":false,"baseRef":"main","headRef":"feature/auth","createdAt":"2026-07-01T12:00:00Z","updatedAt":"2026-07-02T09:00:00Z"},"author":{"login":"dev","name":null,"avatarUrl":null},"commits":[],"changedFiles":[],"statistics":{"fileCount":1,"commitCount":1,"additions":10,"deletions":2,"totalChangedLines":12,"languagesUsed":["C#"],"largestModifiedFile":"Auth.cs","largestModifiedFileChanges":12,"testFilesChanged":0,"sqlFilesChanged":0,"configurationFilesChanged":0,"controllerFilesChanged":0,"serviceFilesChanged":0,"sqlModified":false},"languageBreakdown":{"C#":1},"summary":"Deterministic analysis summary"}
                """;
        }
    }
}
