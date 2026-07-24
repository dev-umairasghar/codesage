using System.Net;
using System.Text.Json;
using CodeSage.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace CodeSage.IntegrationTests.Api;

[Collection(ApiTestCollection.Name)]
public sealed class OpenApiDocumentTests
{
    private readonly HttpClient _client;

    public OpenApiDocumentTests(CodeSageWebApplicationFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task SwaggerV1_IsGenerated_WithExpectedPaths()
    {
        var response = await _client.GetAsync("/swagger/v1/swagger.json");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);

        var root = document.RootElement;
        root.GetProperty("info").GetProperty("title").GetString().Should().Be("CodeSage API");
        root.GetProperty("info").GetProperty("version").GetString().Should().Be("v1");

        var paths = root.GetProperty("paths");
        paths.TryGetProperty("/api/v1/health", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/system/status", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/configuration", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/repositories", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/repositories/{owner}/{name}", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/repositories/{owner}/{name}/pull-requests", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/repositories/{owner}/{name}/pull-requests/{number}", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/repositories/{owner}/{name}/pull-requests/{number}/analysis", out _).Should().BeTrue();
        paths.TryGetProperty("/api/v1/reviews", out _).Should().BeTrue();

        var repoRoot = FindRepoRoot();
        var outputPath = Path.Combine(repoRoot, "docs", "openapi-v1.json");
        Directory.CreateDirectory(Path.GetDirectoryName(outputPath)!);
        await File.WriteAllTextAsync(outputPath, json);

        File.Exists(outputPath).Should().BeTrue();
    }

    private static string FindRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "CodeSage.sln")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException("Could not locate CodeSage.sln from test output.");
    }
}
