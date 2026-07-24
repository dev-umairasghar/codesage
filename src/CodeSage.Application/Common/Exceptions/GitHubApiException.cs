namespace CodeSage.Application.Common.Exceptions;

/// <summary>
/// Represents a failed GitHub API call mapped into the application language.
/// Status codes stay available so the API layer can emit correct ProblemDetails.
/// </summary>
public class GitHubApiException : Exception
{
    public GitHubApiException(string message, int? statusCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    /// <summary>
    /// HTTP status returned by GitHub when available.
    /// </summary>
    public int? StatusCode { get; }
}

/// <summary>
/// Resource was not found on GitHub (HTTP 404).
/// </summary>
public sealed class GitHubNotFoundException(string message)
    : GitHubApiException(message, 404);

/// <summary>
/// GitHub rejected the access token (HTTP 401/403).
/// </summary>
public sealed class GitHubUnauthorizedException(string message)
    : GitHubApiException(message, 401);

/// <summary>
/// GitHub rate limit exceeded (HTTP 403 with rate-limit headers, or 429).
/// </summary>
public sealed class GitHubRateLimitExceededException(string message)
    : GitHubApiException(message, 429);
