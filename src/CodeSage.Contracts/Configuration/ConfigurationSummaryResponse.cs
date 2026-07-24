namespace CodeSage.Contracts.Configuration;

/// <summary>
/// Public configuration summary for clients. Never includes secrets.
/// </summary>
public sealed record ConfigurationSummaryResponse(
    string Application,
    string Version,
    string Environment,
    string GitHubApiBaseUrl,
    bool GitHubTokenConfigured,
    string AiProvider,
    string AiModel,
    string OpenAiBaseUrl,
    bool OpenAiApiKeyConfigured,
    bool ProbeExternalConnectivity,
    bool RequireSecretsAtStartup);
