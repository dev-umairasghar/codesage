using System.Diagnostics;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using CodeSage.Application.Common.Exceptions;
using CodeSage.Application.Features.AI.Abstractions;
using CodeSage.Application.Features.AI.Models;
using CodeSage.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace CodeSage.Infrastructure.AI;

/// <summary>
/// OpenAI Chat Completions adapter.
/// Uses HttpClient (not the official SDK) so Application stays free of vendor types
/// and alternate providers can follow the same IAIProvider shape.
/// </summary>
public sealed class OpenAiProvider(
    HttpClient httpClient,
    IOptions<OpenAiOptions> options,
    ILogger<OpenAiProvider> logger) : IAIProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public string Name => "OpenAI";

    public async Task<AiCompletionResult> CompleteAsync(
        AiCompletionRequest request,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var settings = options.Value;
        if (!settings.HasApiKey)
        {
            throw new AiConfigurationException(
                "Missing OpenAI API key. Set OpenAI:ApiKey via user-secrets "
                + "(dotnet user-secrets set \"OpenAI:ApiKey\" \"sk-...\") "
                + "or environment variable OpenAI__ApiKey. See docs/Configuration.md.");
        }

        var model = request.Model ?? settings.Model;
        var temperature = request.Temperature ?? settings.Temperature;
        var maxTokens = request.MaxTokens ?? settings.MaxTokens;

        // Map CodeSage roles → OpenAI chat roles.
        // Developer instructions are sent as a second system message for broad model compatibility.
        var payload = new OpenAiChatRequest
        {
            Model = model,
            Temperature = temperature,
            MaxTokens = maxTokens,
            ResponseFormat = new OpenAiResponseFormat { Type = "json_object" },
            Messages =
            [
                new OpenAiChatMessage { Role = "system", Content = request.Prompt.SystemPrompt },
                new OpenAiChatMessage { Role = "system", Content = request.Prompt.DeveloperPrompt },
                new OpenAiChatMessage { Role = "user", Content = request.Prompt.UserPrompt }
            ]
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "chat/completions");
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", settings.ApiKey);
        httpRequest.Content = JsonContent.Create(payload, options: SerializerOptions);

        logger.LogInformation(
            "Calling OpenAI chat completions. Model={Model}, Temperature={Temperature}, MaxTokens={MaxTokens}",
            model,
            temperature,
            maxTokens);

        var stopwatch = Stopwatch.StartNew();
        HttpResponseMessage response;
        try
        {
            response = await httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        }
        catch (TaskCanceledException exception) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(exception, "OpenAI request timed out for model {Model}", model);
            throw new AiTimeoutException("The OpenAI request timed out.", exception);
        }
        catch (HttpRequestException exception)
        {
            logger.LogError(exception, "OpenAI network failure for model {Model}", model);
            throw new AiProviderException("Unable to reach the OpenAI API.", statusCode: 502, exception);
        }

        stopwatch.Stop();

        using (response)
        {
            var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                logger.LogWarning("OpenAI rate limit exceeded. Body={Body}", Truncate(body));
                throw new AiRateLimitException("OpenAI rate limit exceeded. Try again later.");
            }

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                logger.LogWarning("OpenAI rejected credentials with status {StatusCode}", (int)response.StatusCode);
                throw new AiConfigurationException("OpenAI rejected the API key or project permissions.");
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "OpenAI returned {StatusCode}. Body={Body}",
                    (int)response.StatusCode,
                    Truncate(body));
                throw new AiProviderException(
                    ExtractErrorMessage(body) ?? $"OpenAI request failed with status {(int)response.StatusCode}.",
                    (int)response.StatusCode);
            }

            OpenAiChatResponse? parsed;
            try
            {
                parsed = JsonSerializer.Deserialize<OpenAiChatResponse>(body, SerializerOptions);
            }
            catch (JsonException exception)
            {
                throw new AiInvalidResponseException("OpenAI returned a malformed response envelope.", exception);
            }

            var content = parsed?.Choices?.FirstOrDefault()?.Message?.Content;
            if (string.IsNullOrWhiteSpace(content))
            {
                throw new AiInvalidResponseException("OpenAI returned an empty completion content.");
            }

            logger.LogInformation(
                "OpenAI chat completions succeeded in {ElapsedMs} ms. Model={Model}, TotalTokens={TotalTokens}",
                stopwatch.ElapsedMilliseconds,
                parsed?.Model ?? model,
                parsed?.Usage?.TotalTokens);

            return new AiCompletionResult(
                Content: content,
                Model: parsed?.Model ?? model,
                PromptTokens: parsed?.Usage?.PromptTokens,
                CompletionTokens: parsed?.Usage?.CompletionTokens,
                TotalTokens: parsed?.Usage?.TotalTokens,
                Duration: stopwatch.Elapsed);
        }
    }

    private static string? ExtractErrorMessage(string body)
    {
        try
        {
            using var document = JsonDocument.Parse(body);
            if (document.RootElement.TryGetProperty("error", out var error)
                && error.TryGetProperty("message", out var message))
            {
                return message.GetString();
            }
        }
        catch (JsonException)
        {
            // ignore
        }

        return null;
    }

    private static string Truncate(string value, int max = 400) =>
        value.Length <= max ? value : value[..max];

    private sealed class OpenAiChatRequest
    {
        public string Model { get; set; } = string.Empty;
        public double Temperature { get; set; }
        public int MaxTokens { get; set; }
        public OpenAiResponseFormat? ResponseFormat { get; set; }
        public List<OpenAiChatMessage> Messages { get; set; } = [];
    }

    private sealed class OpenAiResponseFormat
    {
        public string Type { get; set; } = "json_object";
    }

    private sealed class OpenAiChatMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    private sealed class OpenAiChatResponse
    {
        public string? Model { get; set; }
        public List<OpenAiChoice>? Choices { get; set; }
        public OpenAiUsage? Usage { get; set; }
    }

    private sealed class OpenAiChoice
    {
        public OpenAiChatMessage? Message { get; set; }
    }

    private sealed class OpenAiUsage
    {
        public int? PromptTokens { get; set; }
        public int? CompletionTokens { get; set; }
        public int? TotalTokens { get; set; }
    }
}
