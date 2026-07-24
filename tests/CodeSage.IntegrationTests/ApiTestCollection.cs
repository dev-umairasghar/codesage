using CodeSage.IntegrationTests.Infrastructure;

namespace CodeSage.IntegrationTests;

/// <summary>
/// Shares a single host per test run so Serilog's bootstrap logger is not frozen twice.
/// </summary>
[CollectionDefinition(Name)]
public sealed class ApiTestCollection : ICollectionFixture<CodeSageWebApplicationFactory>
{
    public const string Name = "CodeSageApi";
}
