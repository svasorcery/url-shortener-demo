using Shouldly;
using UrlShortener.Api.Models;
using Xunit;

namespace UrlShortener.Tests.Unit;

public class ShortenUrlRequestValidatorTests
{
    private readonly ShortenUrlRequestValidator _validator = new();

    [Theory]
    [InlineData("https://example.com/path?query=value")]
    [InlineData("http://localhost:5000/path")]
    public void Validate_ShouldAcceptHttpUrl(string url)
    {
        var result = _validator.Validate(new ShortenUrlRequest(url));

        result.IsValid.ShouldBeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com/file")]
    [InlineData("javascript:alert('xss')")]
    public void Validate_ShouldRejectUnsupportedOrMalformedUrl(string url)
    {
        var result = _validator.Validate(new ShortenUrlRequest(url));

        result.IsValid.ShouldBeFalse();
    }
}
