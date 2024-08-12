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
        // POST /shorten
        app.MapPost("shorten", async (
            ShortenUrlRequest request,
            HttpContext httpContext,
            ApplicationDbContext dbContext,
            UrlShorteningService urlShortener,
            ILogger<Program> logger
            ) =>
        {
            try
            {
                var code = await urlShortener.GenerateUniqueCodeAsync();

                var shortenedUrl = new ShortenedUrl
                {
                    Id = Guid.NewGuid(),
                    Code = code,
                    LongUrl = request.Url,
                    ShortUrl = $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/{code}",
                    CreatedAt = DateTime.UtcNow
                };

                dbContext.ShortenedUrls.Add(shortenedUrl);
                await dbContext.SaveChangesAsync();

                logger.LogInformation("Shortened URL with code: '{Code}' stored successfully.", shortenedUrl.Code);

                return Results.Ok(shortenedUrl.ShortUrl);
            }
            catch (Exception exception)
            {
                logger.LogError("Error while storing shortened URL, {Error}.", exception.Message);

                return Results.BadRequest("Unable to store URL, try again later.");
            }
        });

        // GET /A1b2c3
        app.MapGet("{code}", async (
            string code,
            ApplicationDbContext dbContext,
            IMemoryCache cache,
            ILogger<Program> logger
            ) =>
        {
            if (cache.TryGetValue(code, out string? cachedLongUrl) && cachedLongUrl is not null) // [NotNullWhen(true)]
            {
                logger.LogInformation("Shortened URL with code: '{Code}' returned from cache.", code);

                return Results.Redirect(cachedLongUrl);
            }

            var shortenedUrl = await dbContext.ShortenedUrls.FirstOrDefaultAsync(x => x.Code == code);
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
                logger.LogError("Error while caching shortened URL, {Error}.", exception.Message);
            }

            return Results.Redirect(shortenedUrl.LongUrl);
        });
    }

}
