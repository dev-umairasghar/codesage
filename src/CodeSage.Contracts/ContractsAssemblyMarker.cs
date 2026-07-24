namespace CodeSage.Contracts;

/// <summary>
/// Assembly marker for public contracts (DTOs / API shapes shared across hosts and tests).
/// </summary>
public static class ContractsAssemblyMarker
{
    /// <summary>
    /// Gets the Contracts assembly instance.
    /// </summary>
    public static System.Reflection.Assembly Assembly { get; } =
        typeof(ContractsAssemblyMarker).Assembly;
}
