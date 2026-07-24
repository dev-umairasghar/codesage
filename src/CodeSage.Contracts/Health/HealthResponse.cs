namespace CodeSage.Contracts.Health;

/// <summary>
/// Public health-check response contract returned by <c>GET /api/health</c>.
/// </summary>
public sealed record HealthResponse(
    string Status,
    string Application,
    string Version);

/// <summary>
/// Local diagnostics for troubleshooting configuration (never includes secrets).
/// </summary>
public sealed record SystemStatusResponse(
    string Application,
    string Version,
    string Environment,
    string AiProvider,
    string AiModel,
    bool GitHubTokenConfigured,
    bool OpenAiApiKeyConfigured,
    ConnectivityCheckResult GitHubConnectivity,
    ConnectivityCheckResult OpenAiConnectivity,
    IReadOnlyList<string> Diagnostics);

/// <summary>
/// Result of an optional external connectivity probe.
/// </summary>
public sealed record ConnectivityCheckResult(
    string Status,
    string Message,
    int? HttpStatusCode = null);
