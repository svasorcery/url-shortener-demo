namespace UrlShortener.Api.Persistence.Entities;

public class ShortenedUrl
{
    public required Guid Id { get; set; }
    public required string Code { get; set; }
    public required string LongUrl { get; set; }
    public DateTime CreatedAt { get; set; }
}
