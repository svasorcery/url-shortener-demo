using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace UrlShortener.Tests.Integration.Endpoints;

[Collection(PostgreSqlCollection.Name)]
public class UrlShortenerEndpointTests(IntegrationTestWebAppFactory factory)
{
    private readonly HttpClient _httpClient = factory.CreateClient(new WebApplicationFactoryClientOptions
    {
        AllowAutoRedirect = false,
        BaseAddress = new Uri("http://localhost")
    });

    [Fact]
    public async Task Shorten_ShouldCreatePersistedShortUrl_WhenUrlIsValid()
    {
        using var response = await _httpClient.PostAsJsonAsync("/shorten", new
        {
            url = "https://example.com/articles/url-shortening"
        }, TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Created);

        using var body = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var code = body.RootElement.GetProperty("code").GetString();
        var shortUrl = body.RootElement.GetProperty("shortUrl").GetString();

        code.ShouldNotBeNullOrWhiteSpace();
        code.Length.ShouldBe(6);
        code.ShouldMatch("^[0-9A-Za-z]{6}$");
        shortUrl.ShouldBe($"http://localhost/{code}");
        response.Headers.Location.ShouldBe(new Uri(shortUrl!));

        var entity = await factory.DbContext.ShortenedUrls.SingleOrDefaultAsync(
            x => x.Code == code,
            TestContext.Current.CancellationToken);
        entity.ShouldNotBeNull();
        entity.LongUrl.ShouldBe("https://example.com/articles/url-shortening");
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/file")]
    public async Task Shorten_ShouldReturnValidationProblem_WhenUrlIsInvalid(string url)
    {
        var recordsBefore = await factory.DbContext.ShortenedUrls.CountAsync(
            TestContext.Current.CancellationToken);

        using var response = await _httpClient.PostAsJsonAsync(
            "/shorten",
            new { url },
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.BadRequest);
        response.Content.Headers.ContentType?.MediaType.ShouldBe("application/problem+json");
        (await factory.DbContext.ShortenedUrls.CountAsync(TestContext.Current.CancellationToken))
            .ShouldBe(recordsBefore);
    }

    [Fact]
    public async Task Resolve_ShouldRedirectToOriginalUrl_WhenCodeExists()
    {
        const string originalUrl = "https://example.com/destination";
        using var createResponse = await _httpClient.PostAsJsonAsync(
            "/shorten",
            new { url = originalUrl },
            TestContext.Current.CancellationToken);
        using var createBody = JsonDocument.Parse(
            await createResponse.Content.ReadAsStringAsync(TestContext.Current.CancellationToken));
        var code = createBody.RootElement.GetProperty("code").GetString();

        using var response = await _httpClient.GetAsync(
            $"/{code}",
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.Redirect);
        response.Headers.Location.ShouldBe(new Uri(originalUrl));
    }

    [Fact]
    public async Task Resolve_ShouldReturnNotFound_WhenCodeDoesNotExist()
    {
        using var response = await _httpClient.GetAsync(
            "/ABC123",
            TestContext.Current.CancellationToken);

        response.StatusCode.ShouldBe(HttpStatusCode.NotFound);
    }
}
