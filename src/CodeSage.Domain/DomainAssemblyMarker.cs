namespace CodeSage.Domain;

/// <summary>
/// Assembly marker for the Domain project.
/// Domain entities and rules will live here in later stages — intentionally empty in Stage 1.
/// </summary>
public static class DomainAssemblyMarker
{
    /// <summary>
    /// Gets the Domain assembly instance.
    /// </summary>
    public static System.Reflection.Assembly Assembly { get; } =
        typeof(DomainAssemblyMarker).Assembly;
}
