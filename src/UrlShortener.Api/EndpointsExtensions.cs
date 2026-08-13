using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using UrlShortener.Api.Models;
using UrlShortener.Api.Services;
using UrlShortener.Api.Persistence;
using UrlShortener.Api.Persistence.Entities;

namespace UrlShortener.Api;

internal static class EndpointsExtensions
{
    private static readonly MemoryCacheEntryOptions _cacheEntryOptions = new MemoryCacheEntryOptions()
        .SetAbsoluteExpiration(TimeSpan.FromHours(1));

    public static void MapUrlShortenerEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("/shorten", async (
            ShortenUrlRequest request,
            HttpContext httpContext,
            ApplicationDbContext dbContext,
            UrlShorteningService urlShortener,
            IValidator<ShortenUrlRequest> validator,
            CancellationToken cancellationToken,
            ILogger<Program> logger
            ) =>
        {
            var validationResult = await validator.ValidateAsync(request, cancellationToken);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToArray());

                return Results.ValidationProblem(errors);
            }

            try
            {
                var code = await urlShortener.GenerateUniqueCodeAsync(cancellationToken);
                var shortUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{code}";

                var shortenedUrl = new ShortenedUrl
                {
                    Id = Guid.NewGuid(),
                    Code = code,
                    LongUrl = request.Url,
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.ShortenedUrls.Add(shortenedUrl);
                await dbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("Shortened URL with code: '{Code}' stored successfully.", shortenedUrl.Code);

                return Results.Created(shortUrl, new ShortenUrlResponse(code, shortUrl));
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error while storing shortened URL.");

                return Results.Problem("Unable to store URL. Try again later.");
            }
        })
        .WithName("ShortenUrl")
        .Produces<ShortenUrlResponse>(StatusCodes.Status201Created)
        .ProducesValidationProblem()
        .ProducesProblem(StatusCodes.Status500InternalServerError);

        app.MapGet("/{code}", async (
            string code,
            ApplicationDbContext dbContext,
            IMemoryCache cache,
            CancellationToken cancellationToken,
            ILogger<Program> logger
            ) =>
        {
            if (cache.TryGetValue(code, out string? cachedLongUrl) && cachedLongUrl is not null) // [NotNullWhen(true)]
            {
                logger.LogInformation("Shortened URL with code: '{Code}' returned from cache.", code);

                return Results.Redirect(cachedLongUrl);
            }

            var shortenedUrl = await dbContext.ShortenedUrls
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Code == code, cancellationToken);
            if (shortenedUrl is null)
            {
                logger.LogInformation("Shortened URL with code: '{Code}' was not found.", code);

                return Results.NotFound();
            }

            try
            {
                cache.Set(shortenedUrl.Code, shortenedUrl.LongUrl, _cacheEntryOptions);

                logger.LogInformation("Shortened URL with code: '{Code}' cached successfully.", shortenedUrl.Code);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Error while caching shortened URL with code: '{Code}'.", code);
            }

            return Results.Redirect(shortenedUrl.LongUrl);
        })
        .WithName("ResolveShortUrl")
        .Produces(StatusCodes.Status302Found)
        .Produces(StatusCodes.Status404NotFound);
    }

}
