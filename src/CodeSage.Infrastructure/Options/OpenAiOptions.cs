namespace CodeSage.Infrastructure.Options;

/// <summary>
/// OpenAI Chat Completions configuration. Bound from the <c>OpenAI</c> section.
/// Never hardcode secrets — use user-secrets or environment variables.
/// </summary>
public sealed class OpenAiOptions
{
    public const string SectionName = "OpenAI";

    /// <summary>
    /// API key. Prefer <c>OpenAI__ApiKey</c> or user-secrets.
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;

    /// <summary>
    /// API base URL (override for compatible gateways).
    /// </summary>
    public string BaseUrl { get; set; } = "https://api.openai.com/v1/";

    /// <summary>
    /// Default model id (e.g. gpt-4o-mini).
    /// </summary>
    public string Model { get; set; } = "gpt-4o-mini";

    public double Temperature { get; set; } = 0.2;

    public int MaxTokens { get; set; } = 4096;

    public int TimeoutSeconds { get; set; } = 120;

    public bool HasApiKey => !string.IsNullOrWhiteSpace(ApiKey);
}
