using CodeSage.Application;
using CodeSage.Application.Configuration;
using CodeSage.Infrastructure;
using CodeSage.Infrastructure.Options;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace CodeSage.UnitTests.Configuration;

public sealed class StartupValidationTests
{
    [Fact]
    public void Host_FailsFast_WhenSecretsMissingAndRequired()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Application:Name"] = "CodeSage",
            ["Application:Version"] = "0.1.0",
            ["Application:RequireSecretsAtStartup"] = "true",
            ["GitHub:ApiBaseUrl"] = "https://api.github.com/",
            ["GitHub:UserAgent"] = "CodeSage",
            ["GitHub:PersonalAccessToken"] = "",
            ["OpenAI:BaseUrl"] = "https://api.openai.com/v1/",
            ["OpenAI:Model"] = "gpt-4o-mini",
            ["OpenAI:Temperature"] = "0.2",
            ["OpenAI:MaxTokens"] = "100",
            ["OpenAI:TimeoutSeconds"] = "30",
            ["OpenAI:ApiKey"] = ""
        });

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IHostEnvironment>(new FakeHostEnvironment());
        services.AddLogging();
        services.AddApplication(configuration);
        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();

        var act = () => _ = provider.GetRequiredService<IOptions<GitHubOptions>>().Value;

        act.Should().Throw<OptionsValidationException>()
            .Which.Failures.Should().Contain(failure => failure.Contains("Missing GitHub token"));
    }

    [Fact]
    public void Host_Starts_WhenSecretsPresent()
    {
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["Application:Name"] = "CodeSage",
            ["Application:Version"] = "0.1.0",
            ["Application:RequireSecretsAtStartup"] = "true",
            ["GitHub:ApiBaseUrl"] = "https://api.github.com/",
            ["GitHub:UserAgent"] = "CodeSage",
            ["GitHub:PersonalAccessToken"] = "ghp_test",
            ["OpenAI:BaseUrl"] = "https://api.openai.com/v1/",
            ["OpenAI:Model"] = "gpt-4o-mini",
            ["OpenAI:Temperature"] = "0.2",
            ["OpenAI:MaxTokens"] = "100",
            ["OpenAI:TimeoutSeconds"] = "30",
            ["OpenAI:ApiKey"] = "sk-test"
        });

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton<IHostEnvironment>(new FakeHostEnvironment());
        services.AddLogging();
        services.AddApplication(configuration);
        services.AddInfrastructure(configuration);

        using var provider = services.BuildServiceProvider();

        provider.GetRequiredService<IOptions<ApplicationOptions>>().Value.Name.Should().Be("CodeSage");
        provider.GetRequiredService<IOptions<GitHubOptions>>().Value.HasPersonalAccessToken.Should().BeTrue();
        provider.GetRequiredService<IOptions<OpenAiOptions>>().Value.HasApiKey.Should().BeTrue();
    }

    private static IConfiguration BuildConfiguration(Dictionary<string, string?> values) =>
        new ConfigurationBuilder().AddInMemoryCollection(values).Build();

    private sealed class FakeHostEnvironment : IHostEnvironment
    {
        public string EnvironmentName { get; set; } = "Development";
        public string ApplicationName { get; set; } = "CodeSage.Tests";
        public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
        public Microsoft.Extensions.FileProviders.IFileProvider ContentRootFileProvider { get; set; } =
            new Microsoft.Extensions.FileProviders.NullFileProvider();
    }
}
