using Xunit;
using System.Data.Common;
using Testcontainers.PostgreSql;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using UrlShortener.Api.Persistence;

namespace UrlShortener.Tests.Integration;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer;
    private DbConnection? _connection;
    private IServiceScope? _scope;
    public ApplicationDbContext DbContext { get; private set; } = null!;

    public IntegrationTestWebAppFactory()
    {
        _postgreSqlContainer = new PostgreSqlBuilder("postgres:16-alpine")
            .WithCleanUp(true)
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging =>
        {
            logging.SetMinimumLevel(LogLevel.Warning);
            logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning);
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveDbContext<ApplicationDbContext>();
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseNpgsql(_postgreSqlContainer.GetConnectionString());
            });
        });
    }

    public async ValueTask InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        _scope = Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await DbContext.Database.MigrateAsync();
        _connection = DbContext.Database.GetDbConnection();
        await _connection.OpenAsync();
    }

    public override async ValueTask DisposeAsync()
    {
        if (_connection is not null)
        {
            await _connection.CloseAsync();
            await _connection.DisposeAsync();
        }

        _scope?.Dispose();
        await _postgreSqlContainer.DisposeAsync();
        await base.DisposeAsync();
    }
}
