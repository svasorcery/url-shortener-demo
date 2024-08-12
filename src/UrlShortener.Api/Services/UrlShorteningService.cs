using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Persistence;

namespace UrlShortener.Api.Services;

internal class UrlShorteningService(ApplicationDbContext dbContext)
{
    public const int ShortenedUrlLength = 6;

    private const string Alphabet = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";

    private readonly Random _random = new();

    public async Task<string> GenerateUniqueCodeAsync()
    {
        string code = string.Empty;

        do code = GenerateCode();
        while (await dbContext.ShortenedUrls.AnyAsync(x => x.Code == code));

        return code;
    }

    private string GenerateCode()
    {
        var codeChars = new char[ShortenedUrlLength];

        for (var i = 0; i < ShortenedUrlLength; i++)
        {
            var randomIndex = _random.Next(Alphabet.Length - 1);
            codeChars[i] = Alphabet[randomIndex];
        }

        return new string(codeChars);
    }
}
