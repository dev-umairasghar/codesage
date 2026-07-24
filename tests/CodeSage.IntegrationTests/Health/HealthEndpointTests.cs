using System.Net;
using System.Net.Http.Json;
using CodeSage.Contracts.Configuration;
using CodeSage.Contracts.Health;
using CodeSage.IntegrationTests.Infrastructure;
using FluentAssertions;

namespace CodeSage.IntegrationTests.Health;

[Collection(ApiTestCollection.Name)]
public sealed class HealthEndpointTests
{
    private readonly HttpClient _client;

    public HealthEndpointTests(CodeSageWebApplicationFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task GetHealth_Unversioned_ReturnsHealthyPayload()
    {
        var response = await _client.GetAsync("/api/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<HealthResponse>();
        payload.Should().NotBeNull();
        payload!.Status.Should().Be("Healthy");
        payload.Application.Should().Be("CodeSage");
        payload.Version.Should().Be("0.1.0");
    }

    [Fact]
    public async Task GetHealth_V1_ReturnsHealthyPayload()
    {
        var response = await _client.GetAsync("/api/v1/health");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<HealthResponse>();
        payload!.Application.Should().Be("CodeSage");
    }

    [Fact]
    public async Task GetSystemStatus_ReturnsDiagnosticsWithoutSecrets()
    {
        var response = await _client.GetAsync("/api/v1/system/status");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<SystemStatusResponse>();
        payload.Should().NotBeNull();
        payload!.AiProvider.Should().Be("OpenAI");
        payload.AiModel.Should().Be("gpt-4o-mini");
        payload.GitHubTokenConfigured.Should().BeTrue();
        payload.OpenAiApiKeyConfigured.Should().BeTrue();

        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotContain("ghp_test_token_not_real");
        json.Should().NotContain("sk-test-key-not-real");
    }

    [Fact]
    public async Task GetConfiguration_ReturnsPublicSummaryWithoutSecrets()
    {
        var response = await _client.GetAsync("/api/v1/configuration");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var payload = await response.Content.ReadFromJsonAsync<ConfigurationSummaryResponse>();
        payload.Should().NotBeNull();
        payload!.AiModel.Should().Be("gpt-4o-mini");
        payload.GitHubTokenConfigured.Should().BeTrue();
        payload.OpenAiApiKeyConfigured.Should().BeTrue();

        var json = await response.Content.ReadAsStringAsync();
        json.Should().NotContain("ghp_test_token_not_real");
        json.Should().NotContain("sk-test-key-not-real");
    }
}
