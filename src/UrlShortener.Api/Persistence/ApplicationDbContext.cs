using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Persistence.Entities;
using UrlShortener.Api.Services;

namespace UrlShortener.Api.Persistence;

public class ApplicationDbContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<ShortenedUrl> ShortenedUrls { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ShortenedUrl>(builder =>
        {
            builder.HasIndex(x => x.Code)
                .IsUnique();

            builder.Property(x => x.Code)
                .HasMaxLength(UrlShorteningService.ShortenedUrlLength);
        });
    }
}
