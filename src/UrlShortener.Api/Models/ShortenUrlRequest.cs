using FluentValidation;

namespace UrlShortener.Api.Models;

internal record ShortenUrlRequest(string Url);

internal sealed class ShortenUrlRequestValidator : AbstractValidator<ShortenUrlRequest>
{
    public ShortenUrlRequestValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty()
            .Must(uri => Uri.TryCreate(uri, UriKind.Absolute, out _))
            .WithMessage("Invalid URL.");
    }
}