using FluentValidation;

namespace UrlShortener.Api.Models;

internal record ShortenUrlRequest(string Url);

internal sealed class ShortenUrlRequestValidator : AbstractValidator<ShortenUrlRequest>
{
    public ShortenUrlRequestValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty()
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out var parsedUri)
                && (parsedUri.Scheme == Uri.UriSchemeHttp || parsedUri.Scheme == Uri.UriSchemeHttps))
            .WithMessage("Invalid URL.");
    }
}
