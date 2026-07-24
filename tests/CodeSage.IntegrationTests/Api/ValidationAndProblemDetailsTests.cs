using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using CodeSage.IntegrationTests.Infrastructure;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace CodeSage.IntegrationTests.Api;

[Collection(ApiTestCollection.Name)]
public sealed class ValidationAndProblemDetailsTests
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);
    private readonly HttpClient _client;

    public ValidationAndProblemDetailsTests(CodeSageWebApplicationFactory factory) =>
        _client = factory.CreateClient();

    [Fact]
    public async Task GetRepository_WithEmptyOwner_ReturnsProblemDetails()
    {
        // Route constraint still matches; FluentValidation rejects empty owner after binding.
        var response = await _client.GetAsync("/api/v1/repositories/%20/repo");

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(400);
        problem.Title.Should().Be("Validation failed");
        problem.Extensions.Should().ContainKey("errorCode");
        problem.Extensions.Should().ContainKey("errors");
        problem.Extensions.Should().ContainKey("traceId");
    }

    [Fact]
    public async Task GetPullRequest_WithInvalidNumber_IsNotMatchedOrValidated()
    {
        var response = await _client.GetAsync("/api/v1/repositories/acme/app/pull-requests/0");

        // number:int matches 0; FluentValidation requires > 0.
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        problem!.Extensions["errorCode"]!.ToString().Should().Be("validation_failed");
    }

    [Fact]
    public async Task CreateReview_WithEmptyBody_ReturnsProblemDetails()
    {
        using var content = new StringContent("{}", Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("/api/v1/reviews", content);

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>(JsonOptions);
        problem.Should().NotBeNull();
        problem!.Status.Should().Be(400);
        problem.Extensions.Should().ContainKey("errorCode");
        problem.Extensions["errorCode"]!.ToString().Should().Be("validation_failed");
    }
}
