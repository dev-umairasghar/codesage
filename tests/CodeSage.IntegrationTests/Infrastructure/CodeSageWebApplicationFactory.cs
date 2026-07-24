using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace CodeSage.IntegrationTests.Infrastructure;

/// <summary>
/// Boots the API with deterministic test configuration (no external probes).
/// </summary>
public sealed class CodeSageWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        builder.UseSetting(Microsoft.Extensions.Hosting.HostDefaults.EnvironmentKey, "Development");
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Application:Name"] = "CodeSage",
                ["Application:Version"] = "0.1.0",
                ["Application:RequireSecretsAtStartup"] = "true",
                ["Application:ProbeExternalConnectivity"] = "false",
                ["GitHub:PersonalAccessToken"] = "ghp_test_token_not_real",
                ["GitHub:ApiBaseUrl"] = "https://api.github.com/",
                ["GitHub:UserAgent"] = "CodeSage",
                ["OpenAI:ApiKey"] = "sk-test-key-not-real",
                ["OpenAI:BaseUrl"] = "https://api.openai.com/v1/",
                ["OpenAI:Model"] = "gpt-4o-mini",
                ["OpenAI:Temperature"] = "0.2",
                ["OpenAI:MaxTokens"] = "1024",
                ["OpenAI:TimeoutSeconds"] = "30"
            });
        });
    }
}
