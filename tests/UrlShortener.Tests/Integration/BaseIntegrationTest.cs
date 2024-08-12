using Xunit;
using System.Text.Json;
using System.Net.Http.Headers;
using UrlShortener.Api.Persistence;

namespace UrlShortener.Tests.Integration;

public abstract class BaseIntegrationTest : IClassFixture<IntegrationTestWebAppFactory>, IDisposable
{
    protected readonly HttpClient HttpClient;
    protected ApplicationDbContext DbContext { get; private set; } = null!;

    protected BaseIntegrationTest(IntegrationTestWebAppFactory factory)
    {
        HttpClient = factory.CreateClient();
        DbContext = factory.DbContext;
    }

    public void Dispose()
    {
        HttpClient?.Dispose();
    }

    #region Files

    protected static string ReadTestData(string filename)
    {
        var sourceDirectory = Directory.GetParent(AppDomain.CurrentDomain.BaseDirectory)!.Parent!.Parent!.Parent!;
        var path = Path.Combine(sourceDirectory.FullName, "Integration", "Endpoints", "Resources", filename);
        return File.ReadAllText(path);
    }

    #endregion

    #region Http

    protected async Task<T?> SendPostHttpRequestAsync<T>(string url, string content, CancellationToken cancellationToken = default)
        => await SendHttpRequestAsync<T>(HttpMethod.Post, url, content, ensureSuccess: false, cancellationToken);

    protected async Task<T?> SendGetHttpRequestAsync<T>(string url, CancellationToken cancellationToken = default)
        => await SendHttpRequestAsync<T>(HttpMethod.Get, url, content: null, ensureSuccess: false, cancellationToken);

    protected async Task<T?> SendHttpRequestAsync<T>(HttpMethod httpMethod, string url, string? content, bool ensureSuccess, CancellationToken cancellationToken)
    {
        var body = await SendHttpRequestAsync(httpMethod, url, content, ensureSuccess, cancellationToken);
        return JsonSerializer.Deserialize<T>(body);
    }

    protected async Task<string> SendHttpRequestAsync(HttpMethod httpMethod, string url, string? content, bool ensureSuccess, CancellationToken cancellationToken)
    {
        var request = new HttpRequestMessage(httpMethod, url);
        if (content != null)
            request.Content = new StringContent(content, new MediaTypeHeaderValue("application/json"));

        var response = await HttpClient.SendAsync(request, cancellationToken);

        if (ensureSuccess)
            response.EnsureSuccessStatusCode();

        return await response.Content!.ReadAsStringAsync(cancellationToken);
    }

    #endregion
}
