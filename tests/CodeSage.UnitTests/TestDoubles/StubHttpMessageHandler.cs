using System.Net;
using System.Text;

namespace CodeSage.UnitTests.TestDoubles;

/// <summary>
/// Routes HTTP requests to scripted responses for GitHubClient unit tests.
/// Uses longest-substring match so <c>/pulls/42</c> does not steal <c>/pulls/42/files</c>.
/// </summary>
public sealed class StubHttpMessageHandler : HttpMessageHandler
{
    private readonly Dictionary<string, Func<HttpRequestMessage, HttpResponseMessage>> _routes = new(
        StringComparer.OrdinalIgnoreCase);

    private readonly List<HttpRequestMessage> _requests = [];

    public IReadOnlyList<HttpRequestMessage> Requests => _requests;

    public StubHttpMessageHandler MapGet(string pathContains, HttpStatusCode statusCode, string jsonBody)
    {
        _routes[pathContains] = _ => new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(jsonBody, Encoding.UTF8, "application/json")
        };
        return this;
    }

    protected override Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        _requests.Add(request);

        var path = request.RequestUri?.PathAndQuery ?? string.Empty;
        var match = _routes
            .Where(route => path.Contains(route.Key, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(route => route.Key.Length)
            .Select(route => route.Value)
            .FirstOrDefault();

        if (match is not null)
        {
            return Task.FromResult(match(request));
        }

        var notFoundBody = $"{{\"message\":\"Not Found: {path}\"}}";
        return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound)
        {
            Content = new StringContent(notFoundBody, Encoding.UTF8, "application/json")
        });
    }
}
