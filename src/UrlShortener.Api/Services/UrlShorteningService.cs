using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Persistence;

namespace UrlShortener.Api.Services;

internal class UrlShorteningService(ApplicationDbContext dbContext)
{
    public const int ShortenedUrlLength = 6;

    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    public async Task<string> GenerateUniqueCodeAsync(CancellationToken cancellationToken = default)
    {
        string code = string.Empty;

        do code = GenerateCode();
        while (await dbContext.ShortenedUrls.AnyAsync(x => x.Code == code, cancellationToken));

        return code;
    }

    private static string GenerateCode()
        => RandomNumberGenerator.GetString(Alphabet, ShortenedUrlLength);
}
