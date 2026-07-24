namespace CodeSage.Infrastructure;

/// <summary>
/// Assembly marker for Infrastructure (EF Core, GitHub, and other adapters in later stages).
/// </summary>
public static class InfrastructureAssemblyMarker
{
    /// <summary>
    /// Gets the Infrastructure assembly instance.
    /// </summary>
    public static System.Reflection.Assembly Assembly { get; } =
        typeof(InfrastructureAssemblyMarker).Assembly;
}
