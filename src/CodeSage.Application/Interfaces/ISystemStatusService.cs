using CodeSage.Contracts.Health;

namespace CodeSage.Application.Interfaces;

/// <summary>
/// Builds local diagnostics for developers (no secrets).
/// </summary>
public interface ISystemStatusService
{
    Task<SystemStatusResponse> GetStatusAsync(CancellationToken cancellationToken = default);
}
