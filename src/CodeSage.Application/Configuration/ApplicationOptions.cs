namespace CodeSage.Application.Configuration;

/// <summary>
/// Application identity and host metadata. Bound from the <c>Application</c> section.
/// </summary>
public sealed class ApplicationOptions
{
    public const string SectionName = "Application";

    public string Name { get; set; } = "CodeSage";

    public string Version { get; set; } = "0.1.0";

    /// <summary>
    /// Logical environment label (Development, Staging, Production).
    /// When empty, the host environment name is used at runtime.
    /// </summary>
    public string Environment { get; set; } = string.Empty;

    /// <summary>
    /// When true, missing GitHub/OpenAI secrets fail host startup.
    /// Keep true for local runs that need full review capability.
    /// </summary>
    public bool RequireSecretsAtStartup { get; set; } = true;

    /// <summary>
    /// When true, <c>/api/system/status</c> probes GitHub and OpenAI over the network.
    /// </summary>
    public bool ProbeExternalConnectivity { get; set; } = true;
}
