using CodeSage.Application.Configuration;
using CodeSage.Infrastructure.GitHub;
using CodeSage.Infrastructure.Options;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace CodeSage.UnitTests.Configuration;

public sealed class OptionsBindingAndTokenAccessorTests
{
    [Fact]
    public void GitHubOptions_BindsPersonalAccessTokenPresence()
    {
        var options = new GitHubOptions { PersonalAccessToken = "ghp_test" };
        options.HasPersonalAccessToken.Should().BeTrue();

        new GitHubOptions().HasPersonalAccessToken.Should().BeFalse();
    }

    [Fact]
    public void OpenAiOptions_BindsApiKeyPresence()
    {
        var options = new OpenAiOptions { ApiKey = "sk-test" };
        options.HasApiKey.Should().BeTrue();

        new OpenAiOptions().HasApiKey.Should().BeFalse();
    }

    [Fact]
    public async Task ConfiguredGitHubTokenAccessor_ReturnsConfiguredToken()
    {
        var accessor = new ConfiguredGitHubTokenAccessor(Options.Create(new GitHubOptions
        {
            PersonalAccessToken = "ghp_local"
        }));

        var token = await accessor.GetAccessTokenAsync();

        token.Should().Be("ghp_local");
    }

    [Fact]
    public async Task ConfiguredGitHubTokenAccessor_ReturnsNullWhenMissing()
    {
        var accessor = new ConfiguredGitHubTokenAccessor(Options.Create(new GitHubOptions()));

        var token = await accessor.GetAccessTokenAsync();

        token.Should().BeNull();
    }

    [Fact]
    public void ApplicationOptions_DefaultsAreLocalFriendly()
    {
        var options = new ApplicationOptions();

        options.Name.Should().Be("CodeSage");
        options.Version.Should().Be("0.1.0");
        options.RequireSecretsAtStartup.Should().BeTrue();
        options.ProbeExternalConnectivity.Should().BeTrue();
    }
}
