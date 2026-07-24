using CodeSage.Application.Configuration;
using CodeSage.Infrastructure.Options;
using CodeSage.Infrastructure.Options.Validation;
using FluentAssertions;
using Microsoft.Extensions.Options;

namespace CodeSage.UnitTests.Configuration;

public sealed class OptionsValidationTests
{
    [Fact]
    public void ApplicationOptionsValidator_RejectsEmptyName()
    {
        var result = new ApplicationOptionsValidator().Validate(
            null,
            new ApplicationOptions { Name = " ", Version = "0.1.0" });

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(failure => failure.Contains("Application:Name"));
    }

    [Fact]
    public void ApplicationOptionsValidator_AcceptsValidOptions()
    {
        var result = new ApplicationOptionsValidator().Validate(
            null,
            new ApplicationOptions { Name = "CodeSage", Version = "0.1.0" });

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void GitHubOptionsValidator_RequiresTokenWhenStrict()
    {
        var validator = new GitHubOptionsValidator(Options.Create(new ApplicationOptions
        {
            RequireSecretsAtStartup = true
        }));

        var result = validator.Validate(null, new GitHubOptions
        {
            PersonalAccessToken = "",
            ApiBaseUrl = "https://api.github.com/",
            UserAgent = "CodeSage"
        });

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(failure => failure.Contains("Missing GitHub token"));
    }

    [Fact]
    public void GitHubOptionsValidator_AllowsMissingTokenWhenNotStrict()
    {
        var validator = new GitHubOptionsValidator(Options.Create(new ApplicationOptions
        {
            RequireSecretsAtStartup = false
        }));

        var result = validator.Validate(null, new GitHubOptions
        {
            PersonalAccessToken = "",
            ApiBaseUrl = "https://api.github.com/",
            UserAgent = "CodeSage"
        });

        result.Succeeded.Should().BeTrue();
    }

    [Fact]
    public void GitHubOptionsValidator_RejectsInvalidBaseUrl()
    {
        var validator = new GitHubOptionsValidator(Options.Create(new ApplicationOptions
        {
            RequireSecretsAtStartup = false
        }));

        var result = validator.Validate(null, new GitHubOptions
        {
            ApiBaseUrl = "not-a-url",
            UserAgent = "CodeSage"
        });

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(failure => failure.Contains("ApiBaseUrl"));
    }

    [Theory]
    [InlineData(-0.1)]
    [InlineData(2.1)]
    public void OpenAiOptionsValidator_RejectsInvalidTemperature(double temperature)
    {
        var validator = new OpenAiOptionsValidator(Options.Create(new ApplicationOptions
        {
            RequireSecretsAtStartup = false
        }));

        var result = validator.Validate(null, new OpenAiOptions
        {
            ApiKey = "sk-test",
            Model = "gpt-4o-mini",
            Temperature = temperature,
            MaxTokens = 100,
            TimeoutSeconds = 30,
            BaseUrl = "https://api.openai.com/v1/"
        });

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(failure => failure.Contains("Temperature"));
    }

    [Fact]
    public void OpenAiOptionsValidator_RejectsNonPositiveMaxTokens()
    {
        var validator = new OpenAiOptionsValidator(Options.Create(new ApplicationOptions
        {
            RequireSecretsAtStartup = false
        }));

        var result = validator.Validate(null, new OpenAiOptions
        {
            ApiKey = "sk-test",
            Model = "gpt-4o-mini",
            Temperature = 0.2,
            MaxTokens = 0,
            TimeoutSeconds = 30,
            BaseUrl = "https://api.openai.com/v1/"
        });

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(failure => failure.Contains("MaxTokens"));
    }

    [Fact]
    public void OpenAiOptionsValidator_RequiresApiKeyWhenStrict()
    {
        var validator = new OpenAiOptionsValidator(Options.Create(new ApplicationOptions
        {
            RequireSecretsAtStartup = true
        }));

        var result = validator.Validate(null, new OpenAiOptions
        {
            ApiKey = "",
            Model = "gpt-4o-mini",
            Temperature = 0.2,
            MaxTokens = 100,
            TimeoutSeconds = 30,
            BaseUrl = "https://api.openai.com/v1/"
        });

        result.Failed.Should().BeTrue();
        result.Failures.Should().Contain(failure => failure.Contains("Missing OpenAI API key"));
    }

    [Fact]
    public void OpenAiOptionsValidator_AcceptsValidOptions()
    {
        var validator = new OpenAiOptionsValidator(Options.Create(new ApplicationOptions
        {
            RequireSecretsAtStartup = true
        }));

        var result = validator.Validate(null, new OpenAiOptions
        {
            ApiKey = "sk-test",
            Model = "gpt-4o-mini",
            Temperature = 0.2,
            MaxTokens = 4096,
            TimeoutSeconds = 120,
            BaseUrl = "https://api.openai.com/v1/"
        });

        result.Succeeded.Should().BeTrue();
    }
}
