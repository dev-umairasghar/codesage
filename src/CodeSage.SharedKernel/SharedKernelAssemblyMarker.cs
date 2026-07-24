namespace CodeSage.SharedKernel;

/// <summary>
/// Assembly marker for the SharedKernel project.
/// Cross-cutting primitives (e.g. base entity helpers) will be added here only when multiple layers need them.
/// Prefer Domain-specific types until a real shared need appears.
/// </summary>
public static class SharedKernelAssemblyMarker
{
    /// <summary>
    /// Gets the SharedKernel assembly instance (useful for future assembly scanning).
    /// </summary>
    public static System.Reflection.Assembly Assembly { get; } =
        typeof(SharedKernelAssemblyMarker).Assembly;
}
