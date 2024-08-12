using Microsoft.EntityFrameworkCore;

namespace UrlShortener.Api.Persistence;

internal static class DbContextMigrationsExtensions
{
    public static void ApplyDatabaseMigrations(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        dbContext.Database.Migrate();
    }
}
