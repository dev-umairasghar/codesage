namespace CodeSage.Application.Common.Exceptions;

/// <summary>
/// Base type for AI provider failures.
/// </summary>
public class AiException : Exception
{
    public AiException(string message, int? statusCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }

    public int? StatusCode { get; }
}

public sealed class AiTimeoutException(string message, Exception? innerException = null)
    : AiException(message, 504, innerException);

public sealed class AiRateLimitException(string message, Exception? innerException = null)
    : AiException(message, 429, innerException);

public sealed class AiInvalidResponseException(string message, Exception? innerException = null)
    : AiException(message, 502, innerException);

public sealed class AiConfigurationException(string message)
    : AiException(message, 503);

public sealed class AiProviderException(string message, int? statusCode = null, Exception? innerException = null)
    : AiException(message, statusCode, innerException);
