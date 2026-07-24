namespace CodeSage.Application;

/// <summary>
/// Assembly marker for the Application layer (use cases, ports, pipeline behaviors).
/// </summary>
public static class ApplicationAssemblyMarker
{
    /// <summary>
    /// Gets the Application assembly instance.
    /// </summary>
    public static System.Reflection.Assembly Assembly { get; } =
        typeof(ApplicationAssemblyMarker).Assembly;
}
