using System.Net;
using System.Text;
using CodeSage.Application.Common.Exceptions;
using CodeSage.Application.Features.AI.Models;
using CodeSage.Infrastructure.AI;
using CodeSage.Infrastructure.Options;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CodeSage.UnitTests.AI;

public sealed class OpenAiProviderTests
{
    [Fact]
    public async Task CompleteAsync_MapsOpenAiResponse_ToAiCompletionResult()
    {
        var sut = CreateProvider(new OpenAiStubHandler(HttpStatusCode.OK,
            """
            {
              "model": "gpt-4o-mini",
              "choices": [
                { "message": { "role": "assistant", "content": "{\"reviewSummary\":\"ok\"}" } }
              ],
              "usage": { "prompt_tokens": 11, "completion_tokens": 22, "total_tokens": 33 }
            }
            """));

        var result = await sut.CompleteAsync(new AiCompletionRequest(new AiPrompt("sys", "dev", "user")));

        result.Content.Should().Contain("reviewSummary");
        result.Model.Should().Be("gpt-4o-mini");
        result.PromptTokens.Should().Be(11);
        result.CompletionTokens.Should().Be(22);
        result.TotalTokens.Should().Be(33);
        result.Duration.Should().BeGreaterThan(TimeSpan.Zero);
    }

    [Fact]
    public async Task CompleteAsync_WithoutApiKey_ThrowsAiConfigurationException()
    {
        var sut = CreateProvider(new OpenAiStubHandler(HttpStatusCode.OK, "{}"), apiKey: "");

        var act = async () => await sut.CompleteAsync(new AiCompletionRequest(new AiPrompt("s", "d", "u")));

        await act.Should().ThrowAsync<AiConfigurationException>();
    }

    [Fact]
    public async Task CompleteAsync_WhenRateLimited_ThrowsAiRateLimitException()
    {
        var sut = CreateProvider(new OpenAiStubHandler(HttpStatusCode.TooManyRequests, """{"error":{"message":"rate"}}"""));

        var act = async () => await sut.CompleteAsync(new AiCompletionRequest(new AiPrompt("s", "d", "u")));

        await act.Should().ThrowAsync<AiRateLimitException>();
    }

    [Fact]
    public async Task CompleteAsync_WhenEmptyContent_ThrowsAiInvalidResponseException()
    {
        var sut = CreateProvider(new OpenAiStubHandler(HttpStatusCode.OK,
            """{"model":"gpt","choices":[{"message":{"role":"assistant","content":""}}],"usage":{}}"""));

        var act = async () => await sut.CompleteAsync(new AiCompletionRequest(new AiPrompt("s", "d", "u")));

        await act.Should().ThrowAsync<AiInvalidResponseException>();
    }

    private static OpenAiProvider CreateProvider(HttpMessageHandler handler, string apiKey = "sk-test")
    {
        var client = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://api.openai.com/v1/")
        };

        return new OpenAiProvider(
            client,
            Options.Create(new OpenAiOptions
            {
                ApiKey = apiKey,
                Model = "gpt-4o-mini",
                Temperature = 0.2,
                MaxTokens = 1000,
                TimeoutSeconds = 30
            }),
            NullLogger<OpenAiProvider>.Instance);
    }

    private sealed class OpenAiStubHandler(HttpStatusCode statusCode, string body) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json")
            });
    }
}
