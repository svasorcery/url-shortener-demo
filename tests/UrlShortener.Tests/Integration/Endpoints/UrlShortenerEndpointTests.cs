using Xunit;
using Shouldly;
using System.Text.Json;
using UrlShortener.Api.Models;
using UrlShortener.Api.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Tests.Integration.Endpoints;

public class UrlShortenerEndpointTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private const string EndpointUrl = "/shorten";

    [Fact]
    public async Task Shorten_ShouldReturnShortUrl_WhenProvidedUrlIsValid()
    {
        var requestContent = ReadTestData("ShortenUrlSuccessRequest.json");
        var requestDto = JsonSerializer.Deserialize<ShortenUrlRequest>(requestContent);

        requestContent.ShouldNotBeNullOrWhiteSpace();
        requestDto.ShouldNotBeNull();
        requestDto.Url.ShouldNotBeNull();

        var response = await SendPostHttpRequestAsync<string>(EndpointUrl, requestContent);

        response.ShouldNotBeNull();

        var code = response[6..];
        var entity = await FindShortenUrlAsync(code);

        entity.ShouldNotBeNull();
        entity.LongUrl.ShouldBe(requestDto.Url);
    }

    private Task<ShortenedUrl?> FindShortenUrlAsync(string code)
        => DbContext.ShortenedUrls
            .FirstOrDefaultAsync(x => x.Code == code, cancellationToken: default);
}
