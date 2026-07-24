using CodeSage.Infrastructure.Options;
using Microsoft.Extensions.Options;

namespace CodeSage.Infrastructure.Options.Validation;

/// <summary>
/// Fail-fast validation for GitHub configuration.
/// </summary>
public sealed class GitHubOptionsValidator(IOptions<Application.Configuration.ApplicationOptions> applicationOptions)
    : IValidateOptions<GitHubOptions>
{
    public ValidateOptionsResult Validate(string? name, GitHubOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (!Uri.TryCreate(options.ApiBaseUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            failures.Add(
                "GitHub:ApiBaseUrl must be an absolute http(s) URL (example: https://api.github.com/).");
        }

        if (string.IsNullOrWhiteSpace(options.UserAgent))
        {
            failures.Add("GitHub:UserAgent is required (GitHub rejects anonymous clients).");
        }

        if (applicationOptions.Value.RequireSecretsAtStartup && !options.HasPersonalAccessToken)
        {
            failures.Add(
                "Missing GitHub token. Set GitHub:PersonalAccessToken via user-secrets "
                + "(dotnet user-secrets set \"GitHub:PersonalAccessToken\" \"ghp_...\") "
                + "or environment variable GitHub__PersonalAccessToken. "
                + "See docs/Configuration.md.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}

/// <summary>
/// Fail-fast validation for OpenAI configuration.
/// </summary>
public sealed class OpenAiOptionsValidator(IOptions<Application.Configuration.ApplicationOptions> applicationOptions)
    : IValidateOptions<OpenAiOptions>
{
    public ValidateOptionsResult Validate(string? name, OpenAiOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var failures = new List<string>();

        if (!Uri.TryCreate(options.BaseUrl, UriKind.Absolute, out var uri)
            || (uri.Scheme != Uri.UriSchemeHttps && uri.Scheme != Uri.UriSchemeHttp))
        {
            failures.Add(
                "OpenAI:BaseUrl must be an absolute http(s) URL (example: https://api.openai.com/v1/).");
        }

        if (string.IsNullOrWhiteSpace(options.Model))
        {
            failures.Add("OpenAI:Model is required (example: gpt-4o-mini).");
        }

        if (options.Temperature is < 0 or > 2)
        {
            failures.Add("OpenAI:Temperature must be between 0 and 2.");
        }

        if (options.MaxTokens <= 0)
        {
            failures.Add("OpenAI:MaxTokens must be greater than zero.");
        }

        if (options.TimeoutSeconds is <= 0 or > 600)
        {
            failures.Add("OpenAI:TimeoutSeconds must be between 1 and 600.");
        }

        if (applicationOptions.Value.RequireSecretsAtStartup && !options.HasApiKey)
        {
            failures.Add(
                "Missing OpenAI API key. Set OpenAI:ApiKey via user-secrets "
                + "(dotnet user-secrets set \"OpenAI:ApiKey\" \"sk-...\") "
                + "or environment variable OpenAI__ApiKey. "
                + "See docs/Configuration.md.");
        }

        return failures.Count == 0
            ? ValidateOptionsResult.Success
            : ValidateOptionsResult.Fail(failures);
    }
}
