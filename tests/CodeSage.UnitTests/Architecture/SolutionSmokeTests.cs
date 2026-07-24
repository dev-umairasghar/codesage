using CodeSage.Application;
using CodeSage.Domain;
using CodeSage.SharedKernel;
using FluentAssertions;

namespace CodeSage.UnitTests.Architecture;

/// <summary>
/// Smoke tests that prove the solution layers compile and load correctly.
/// </summary>
public sealed class SolutionSmokeTests
{
    [Fact]
    public void Domain_Assembly_Is_Loadable()
    {
        DomainAssemblyMarker.Assembly.GetName().Name
            .Should().Be("CodeSage.Domain");
    }

    [Fact]
    public void Application_Assembly_Is_Loadable()
    {
        ApplicationAssemblyMarker.Assembly.GetName().Name
            .Should().Be("CodeSage.Application");
    }

    [Fact]
    public void SharedKernel_Assembly_Is_Loadable()
    {
        SharedKernelAssemblyMarker.Assembly.GetName().Name
            .Should().Be("CodeSage.SharedKernel");
    }
}
