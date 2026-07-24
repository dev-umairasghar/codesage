using System.Diagnostics;
using System.Text.Json;
using CodeSage.Application.Common.Exceptions;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CodeSage.Api.Middleware;

/// <summary>
/// Converts unhandled exceptions into RFC 7807 <see cref="ProblemDetails"/> responses.
/// </summary>
public sealed class ExceptionHandlingMiddleware(
    RequestDelegate next,
    ILogger<ExceptionHandlingMiddleware> logger,
    IHostEnvironment environment)
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            await WriteProblemAsync(context, exception).ConfigureAwait(false);
        }
    }

    private async Task WriteProblemAsync(HttpContext context, Exception exception)
    {
        var (statusCode, title, detail, errorCode, extensions) = MapException(exception, context);

        if (statusCode >= StatusCodes.Status500InternalServerError)
        {
            logger.LogError(
                exception,
                "Unhandled exception for {Method} {Path} ErrorCode={ErrorCode}",
                context.Request.Method,
                context.Request.Path,
                errorCode);
        }
        else if (statusCode == StatusCodes.Status400BadRequest)
        {
            logger.LogWarning(
                "Validation failed for {Method} {Path} ErrorCode={ErrorCode}",
                context.Request.Method,
                context.Request.Path,
                errorCode);
        }
        else
        {
            logger.LogWarning(
                exception,
                "Request failed for {Method} {Path} ErrorCode={ErrorCode} Status={StatusCode}",
                context.Request.Method,
                context.Request.Path,
                errorCode,
                statusCode);
        }

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = context.Request.Path,
            Type = $"https://httpstatuses.com/{statusCode}"
        };

        problem.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;
        problem.Extensions["errorCode"] = errorCode;

        foreach (var (key, value) in extensions)
        {
            problem.Extensions[key] = value;
        }

        if (!environment.IsDevelopment() && statusCode >= StatusCodes.Status500InternalServerError)
        {
            problem.Detail = "An unexpected error occurred.";
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = statusCode;

        await context.Response
            .WriteAsync(JsonSerializer.Serialize(problem, SerializerOptions), context.RequestAborted)
            .ConfigureAwait(false);
    }

    private static (int StatusCode, string Title, string Detail, string ErrorCode, Dictionary<string, object?> Extensions)
        MapException(Exception exception, HttpContext context)
    {
        if (exception is OperationCanceledException && context.RequestAborted.IsCancellationRequested)
        {
            return (
                StatusCodes.Status499ClientClosedRequest,
                "Request aborted",
                "The client closed the connection before the server finished processing.",
                "request_aborted",
                []);
        }

        return exception switch
        {
            ValidationException validationException => (
                StatusCodes.Status400BadRequest,
                "Validation failed",
                "One or more validation errors occurred.",
                "validation_failed",
                new Dictionary<string, object?>
                {
                    ["errors"] = validationException.Errors
                        .GroupBy(error => string.IsNullOrWhiteSpace(error.PropertyName) ? "_" : error.PropertyName)
                        .ToDictionary(
                            group => group.Key,
                            group => group.Select(error => error.ErrorMessage).ToArray())
                }),
            OptionsValidationException optionsValidation => (
                StatusCodes.Status503ServiceUnavailable,
                "Configuration validation failed",
                string.Join(" ", optionsValidation.Failures),
                "configuration_invalid",
                new Dictionary<string, object?>
                {
                    ["optionsName"] = optionsValidation.OptionsName,
                    ["failures"] = optionsValidation.Failures.ToArray()
                }),
            GitHubNotFoundException notFound => (
                StatusCodes.Status404NotFound,
                "GitHub resource not found",
                notFound.Message,
                "github_not_found",
                []),
            GitHubUnauthorizedException unauthorized => (
                StatusCodes.Status401Unauthorized,
                "GitHub authorization failed",
                unauthorized.Message,
                "github_unauthorized",
                []),
            GitHubRateLimitExceededException rateLimit => (
                StatusCodes.Status429TooManyRequests,
                "GitHub rate limit exceeded",
                rateLimit.Message,
                "github_rate_limited",
                []),
            GitHubApiException gitHubApiException => (
                gitHubApiException.StatusCode ?? StatusCodes.Status502BadGateway,
                "GitHub API error",
                gitHubApiException.Message,
                "github_error",
                []),
            AiTimeoutException timeout => (
                StatusCodes.Status504GatewayTimeout,
                "AI provider timeout",
                timeout.Message,
                "ai_timeout",
                []),
            AiRateLimitException rateLimit => (
                StatusCodes.Status429TooManyRequests,
                "AI provider rate limit exceeded",
                rateLimit.Message,
                "ai_rate_limited",
                []),
            AiInvalidResponseException invalidResponse => (
                StatusCodes.Status502BadGateway,
                "AI provider returned an invalid response",
                invalidResponse.Message,
                "ai_invalid_response",
                []),
            AiConfigurationException configuration => (
                StatusCodes.Status503ServiceUnavailable,
                "AI provider is not configured",
                configuration.Message,
                "ai_configuration",
                []),
            AiException aiException => (
                aiException.StatusCode ?? StatusCodes.Status502BadGateway,
                "AI provider error",
                aiException.Message,
                "ai_error",
                []),
            UnauthorizedAccessException unauthorizedAccess => (
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                unauthorizedAccess.Message,
                "unauthorized",
                []),
            BadHttpRequestException badHttpRequestException => (
                badHttpRequestException.StatusCode,
                "Bad request",
                badHttpRequestException.Message,
                "bad_request",
                []),
            _ => (
                StatusCodes.Status500InternalServerError,
                "Internal server error",
                exception.Message,
                "internal_error",
                [])
        };
    }
}
